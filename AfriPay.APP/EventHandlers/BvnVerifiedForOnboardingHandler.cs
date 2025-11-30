using AfriPay.CORE.Entities;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.EventHandlers
{
    public class BvnVerifiedForOnboardingHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<BvnVerifiedForOnboardingHandler> _logger;

        public BvnVerifiedForOnboardingHandler(
            IUnitOfWork unitOfWork,
            IEventPublisher eventPublisher,
            ILogger<BvnVerifiedForOnboardingHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(BvnVerifiedForOnboardingEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating customer for onboarding: {OnboardingId}", @event.OnboardingId);

                var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(@event.OnboardingId, cancellationToken);
                if (request == null)
                {
                    _logger.LogWarning("Onboarding request not found: {OnboardingId}", @event.OnboardingId);
                    return;
                }

                // Create customer
                var customer = Customer.Create(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.BVN
                );

                customer.VerifyBvn();

                await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Link customer to onboarding request
                request.LinkCustomer(customer.CustomerId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish customer created events
                await _eventPublisher.PublishManyAsync(customer.DomainEvents, cancellationToken);
                customer.ClearDomainEvents();

                _logger.LogInformation("Customer created: {CustomerReference}", customer.CustomerReference);

                // Trigger virtual account creation
                request.MarkVirtualAccountCreationPending();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // This would trigger the next handler to create virtual account
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for onboarding: {OnboardingId}", @event.OnboardingId);
            }
        }
    }
}
