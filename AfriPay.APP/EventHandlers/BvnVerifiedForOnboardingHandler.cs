using AfriPay.APP.Services;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class BvnVerifiedForOnboardingHandler : INotificationHandler<BvnVerifiedForOnboardingEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVirtualAccountProvider _virtualAccountProvider;
    private readonly ILogger<BvnVerifiedForOnboardingHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public BvnVerifiedForOnboardingHandler(
        IUnitOfWork unitOfWork,
        IVirtualAccountProvider virtualAccountProvider,
        IPasswordHasher passwordHasher,
        ILogger<BvnVerifiedForOnboardingHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _virtualAccountProvider = virtualAccountProvider ?? throw new ArgumentNullException(nameof(virtualAccountProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(BvnVerifiedForOnboardingEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing BVN verified event for onboarding: {OnboardingId}", notification.OnboardingId);

            // 1. Get onboarding request
            var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(notification.OnboardingId, cancellationToken);
            if (request == null)
            {
                _logger.LogWarning("Onboarding request not found: {OnboardingId}", notification.OnboardingId);
                return;
            }

            // 2. Create customer
            _logger.LogInformation("Creating customer for: {FirstName} {LastName}", request.FirstName, request.LastName);
            var password = _passwordHasher.HashPassword("Password123");
            var customer = Customer.Create(
                request.FirstName,
                request.LastName,
                request.ContactInfo.Email,
                request.ContactInfo.PhoneNumber,
                password,
                request.BVN!
                );

            customer.VerifyBvn();
            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);

            // 3. Link customer to onboarding request
            request.LinkCustomer(customer.CustomerId);

            // 4. Mark virtual account creation pending
            request.MarkVirtualAccountCreationPending();

            // 5. Create virtual account via provider
            _logger.LogInformation("Creating virtual account for customer: {CustomerId}", customer.CustomerId.Value);

            var virtualAccountResult = await _virtualAccountProvider.CreateVirtualAccountAsync(
                customer.CustomerReference.Value,
                request.FirstName,
                request.LastName,
                request.ContactInfo.Email,
                request.ContactInfo.PhoneNumber,
                cancellationToken);

            if (virtualAccountResult.IsSuccess)
            {
                var virtualAccount = virtualAccountResult.Value;

                // 6. Create account entity using the correct factory method
                var account = Account.CreateVirtualAccount(
                    customerId: customer.CustomerId,
                    accountNumber: AccountNumber.Create(virtualAccount.AccountNumber),
                    providerReference: virtualAccount.ProviderReference);

                await _unitOfWork.Accounts.AddAsync(account, cancellationToken);

                // 7. Link virtual account to onboarding request
                request.LinkVirtualAccount(account.AccountId);

                // 8. Complete onboarding
                request.Complete();

                _logger.LogInformation(
                    "Virtual account created: {AccountNumber}, Provider: {ProviderReference}",
                    virtualAccount.AccountNumber,
                    virtualAccount.ProviderReference);
            }
            else
            {
                request.MarkVirtualAccountCreationFailed(virtualAccountResult.Error ?? "Unknown error");
                _logger.LogError("Failed to create virtual account: {Error}", virtualAccountResult.Error);
            }

            // 9. Save all changes - automatically dispatches domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Onboarding completed for: {OnboardingId}, Status: {Status}",
                notification.OnboardingId, request.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BVN verified event for onboarding: {OnboardingId}", notification.OnboardingId);
            throw;
        }
    }
}