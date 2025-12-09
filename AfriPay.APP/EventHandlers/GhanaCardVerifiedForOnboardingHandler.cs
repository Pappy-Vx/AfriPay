using AfriPay.APP.Services;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers
{
    /// <summary>
    /// Handles the GhanaCardVerifiedForOnboardingEvent by creating the customer, 
    /// generating a virtual account, and completing the onboarding process for Ghanaian users.
    /// </summary>
    public class GhanaCardVerifiedForOnboardingHandler
        : INotificationHandler<GhanaCardVerifiedForOnboardingEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVirtualAccountProvider _virtualAccountProvider;
        private readonly ILogger<GhanaCardVerifiedForOnboardingHandler> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public GhanaCardVerifiedForOnboardingHandler(
            IUnitOfWork unitOfWork,
            IVirtualAccountProvider virtualAccountProvider,
            IPasswordHasher passwordHasher,
            ILogger<GhanaCardVerifiedForOnboardingHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _virtualAccountProvider = virtualAccountProvider ?? throw new ArgumentNullException(nameof(virtualAccountProvider));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(GhanaCardVerifiedForOnboardingEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing Ghana Card verified event for onboarding: {OnboardingId}",
                    notification.OnboardingId);

                // 1. Retrieve the onboarding request
                var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(
                    notification.OnboardingId, cancellationToken);

                if (request == null)
                {
                    _logger.LogWarning("Onboarding request not found: {OnboardingId}", notification.OnboardingId);
                    return;
                }

                // 2. Create Customer (UserTag will be set later by the user)
                _logger.LogInformation(
                    "Creating customer for Ghana: {FirstName} {LastName}",
                    request.FirstName, request.LastName);

                //var defaultPassword = _passwordHasher.HashPassword("Password123"); // Will be forced to change on first login

                var customer = Customer.Create(
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    email: request.ContactInfo.Email,
                    phoneNumber: request.ContactInfo.PhoneNumber,
                    //passwordHash: defaultPassword,
                    identity: notification.GhanaCard);

                // Set default transfer PIN '0000' for newly onboarded customers
                var defaultPinHash = _passwordHasher.HashPassword("0000");
                customer.SetTransferPin(defaultPinHash);

                customer.MarkIdentityVerified(); // Marks identity as verified for Ghana

                await _unitOfWork.Customers.AddAsync(customer, cancellationToken);

                // 3. Link customer to onboarding request
                request.LinkCustomer(customer.CustomerId);

                // 4. Mark virtual account creation as pending
                request.MarkVirtualAccountCreationPending();

                // 5. Create virtual account via provider (e.g., Mono, Seamfix, etc.)
                _logger.LogInformation(
                    "Creating virtual account for Ghanaian customer: {CustomerId}",
                    customer.CustomerId.Value);

                var virtualAccountResult = await _virtualAccountProvider.CreateVirtualAccountAsync(
                    customerReference: customer.CustomerReference.Value,
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    email: request.ContactInfo.Email,
                    phoneNumber: request.ContactInfo.PhoneNumber,
                    cancellationToken: cancellationToken);

                if (virtualAccountResult.IsSuccess)
                {
                    var virtualAccount = virtualAccountResult.Value;

                    // 6. Create Account entity using factory method
                    var account = Account.CreateVirtualAccount(
                        customerId: customer.CustomerId,
                        accountNumber: AccountNumber.Create(virtualAccount.AccountNumber),
                        providerReference: virtualAccount.ProviderReference,
                        currency: "GHS");

                    await _unitOfWork.Accounts.AddAsync(account, cancellationToken);

                    // 7. Link virtual account to onboarding request
                    request.LinkVirtualAccount(account.AccountId);

                    // 8. Complete onboarding
                    request.Complete();

                    _logger.LogInformation(
                        "Ghana onboarding completed successfully. Virtual Account: {AccountNumber} (Ref: {ProviderRef})",
                        virtualAccount.AccountNumber,
                        virtualAccount.ProviderReference);
                }
                else
                {
                    request.MarkVirtualAccountCreationFailed(
                        virtualAccountResult.Error ?? "Failed to create virtual account with provider");

                    _logger.LogError(
                        "Virtual account creation failed for Ghana onboarding {OnboardingId}: {Error}",
                        notification.OnboardingId,
                        virtualAccountResult.Error);
                }

                // 9. Persist all changes (this also publishes domain events like OnboardingCompletedEvent)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "GhanaCard onboarding process finished for {OnboardingId}. Final status: {Status}",
                    notification.OnboardingId,
                    request.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception in GhanaCardVerifiedForOnboardingHandler for OnboardingId: {OnboardingId}",
                    notification.OnboardingId);

                // Optionally re-throw or publish a failed event depending on your retry strategy
                throw;
            }
        }
    }
}