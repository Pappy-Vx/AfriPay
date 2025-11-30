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
    public class OnboardingRequestedHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBvnVerificationService _bvnService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OnboardingRequestedHandler> _logger;

        public OnboardingRequestedHandler(
            IUnitOfWork unitOfWork,
            IBvnVerificationService bvnService,
            IEventPublisher eventPublisher,
            ILogger<OnboardingRequestedHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _bvnService = bvnService ?? throw new ArgumentNullException(nameof(bvnService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(OnboardingRequestedEvent @event, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing onboarding request: {RequestReference}", @event.RequestReference);

                var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(@event.OnboardingId, cancellationToken);
                if (request == null)
                {
                    _logger.LogWarning("Onboarding request not found: {OnboardingId}", @event.OnboardingId);
                    return;
                }

                // Mark as pending BVN verification
                request.MarkBvnVerificationPending();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Call BVN verification service
                var verificationResult = await _bvnService.VerifyBvnAsync(
                    @event.BVN.Value,
                    @event.FirstName,
                    @event.LastName,
                    cancellationToken
                );

                if (verificationResult.IsSuccess && verificationResult.Value.IsVerified)
                {
                    request.MarkBvnVerified();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _eventPublisher.PublishManyAsync(request.DomainEvents, cancellationToken);
                    request.ClearDomainEvents();

                    _logger.LogInformation("BVN verified for onboarding: {RequestReference}", @event.RequestReference);
                }
                else
                {
                    request.MarkBvnVerificationFailed(verificationResult.Error ?? "BVN verification failed");
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("BVN verification failed for: {RequestReference}", @event.RequestReference);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing onboarding request: {RequestReference}", @event.RequestReference);
            }
        }
    }

}
