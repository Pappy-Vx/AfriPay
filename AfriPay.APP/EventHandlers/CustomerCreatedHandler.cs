using AfriPay.CORE.Entities;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.EventHandlers
{
    public class CustomerCreatedHandler : INotificationHandler<CustomerCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVirtualAccountProvider _vaProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<CustomerCreatedHandler> _logger;

        public CustomerCreatedHandler(
            IUnitOfWork unitOfWork,
            IVirtualAccountProvider vaProvider,
            IEventPublisher eventPublisher,
            ILogger<CustomerCreatedHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _vaProvider = vaProvider ?? throw new ArgumentNullException(nameof(vaProvider));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(CustomerCreatedEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating virtual account for customer: {CustomerReference}", @event.CustomerReference);

                var customer = await _unitOfWork.Customers.GetByIdAsync(@event.CustomerId, cancellationToken);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", @event.CustomerId);
                    return;
                }

                // Find associated onboarding request using BVN
                var onboardingRequest = await _unitOfWork.OnboardingRequests
                    .GetByBvnAsync(customer.BVN.Value, cancellationToken);

                if (onboardingRequest == null)
                {
                    _logger.LogWarning("No onboarding request found for customer: {CustomerReference}", @event.CustomerReference);
                    return;
                }

                _logger.LogInformation("Found onboarding request: {RequestReference}", onboardingRequest.RequestReference);

                // Mark as pending virtual account creation
                onboardingRequest.MarkVirtualAccountCreationPending();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Calling virtual account provider for customer: {CustomerReference}", @event.CustomerReference);

                // Call virtual account provider
                var vaResult = await _vaProvider.CreateVirtualAccountAsync(
                    @event.CustomerReference.Value,
                    @event.FirstName,
                    @event.LastName,
                    @event.Email,
                    customer.PhoneNumber,
                    cancellationToken
                );

                if (vaResult.IsSuccess && vaResult.Value.IsSuccess)
                {
                    var vaResponse = vaResult.Value;

                    _logger.LogInformation("Virtual account created: {AccountNumber} for customer: {CustomerReference}",
                        vaResponse.AccountNumber, @event.CustomerReference);

                    // Create virtual account entity
                    var virtualAccount = Account.CreateVirtualAccount(
                        customer.CustomerId,
                        AccountNumber.Create(vaResponse.AccountNumber),
                        vaResponse.ProviderReference
                    );

                    await _unitOfWork.Accounts.AddAsync(virtualAccount, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Virtual account entity saved: {AccountId}", virtualAccount.AccountId);

                    // Activate the customer now that virtual account is created
                    customer.Activate();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Customer activated: {CustomerReference}", customer.CustomerReference);

                    // Link to onboarding request
                    onboardingRequest.LinkVirtualAccount(virtualAccount.AccountId);
                    onboardingRequest.Complete();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Onboarding completed: {RequestReference}", onboardingRequest.RequestReference);

                    // Publish events
                    var allEvents = virtualAccount.DomainEvents.Concat(onboardingRequest.DomainEvents).ToList();

                    virtualAccount.ClearDomainEvents();
                    onboardingRequest.ClearDomainEvents();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _eventPublisher.PublishManyAsync(allEvents, cancellationToken);
                }
                else
                {
                    var errorMessage = vaResult.Error ?? vaResult.Value?.ErrorMessage ?? "Virtual account creation failed";

                    _logger.LogError("Virtual account creation failed for customer: {CustomerReference}. Error: {Error}",
                        @event.CustomerReference, errorMessage);

                    onboardingRequest.MarkVirtualAccountCreationFailed(errorMessage);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating virtual account for customer: {CustomerReference}", @event.CustomerReference);

                // Try to mark the onboarding as failed
                try
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(@event.CustomerId, cancellationToken);
                    if (customer != null)
                    {
                        var onboardingRequest = await _unitOfWork.OnboardingRequests
                            .GetByBvnAsync(customer.BVN.Value, cancellationToken);

                        if (onboardingRequest != null)
                        {
                            onboardingRequest.Fail($"Virtual account creation error: {ex.Message}");
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to mark onboarding as failed");
                }
            }
        }
    }
}
