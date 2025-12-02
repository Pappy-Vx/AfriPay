using AfriPay.CORE.Entities;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class BvnVerifiedForOnboardingHandler : INotificationHandler<BvnVerifiedForOnboardingEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BvnVerifiedForOnboardingHandler> _logger;

    public BvnVerifiedForOnboardingHandler(
        IUnitOfWork unitOfWork,
        ILogger<BvnVerifiedForOnboardingHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(BvnVerifiedForOnboardingEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating customer for onboarding: {OnboardingId}", notification.OnboardingId);

            var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(notification.OnboardingId, cancellationToken);
            if (request == null)
            {
                _logger.LogWarning("Onboarding request not found: {OnboardingId}", notification.OnboardingId);
                return;
            }

            // Create customer
            var customer = Customer.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.BVN);

            customer.VerifyBvn();

            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);

            // Link customer to onboarding request
            request.LinkCustomer(customer.CustomerId);

            // Mark virtual account creation pending
            request.MarkVirtualAccountCreationPending();

            // Single SaveChangesAsync - automatically dispatches all domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer created: {CustomerReference}", customer.CustomerReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer for onboarding: {OnboardingId}", notification.OnboardingId);
        }
    }
}