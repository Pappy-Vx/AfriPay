using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class OnboardingRequestedHandler : INotificationHandler<OnboardingRequestedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBvnVerificationService _bvnService;
    private readonly ILogger<OnboardingRequestedHandler> _logger;

    public OnboardingRequestedHandler(
        IUnitOfWork unitOfWork,
        IBvnVerificationService bvnService,
        ILogger<OnboardingRequestedHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _bvnService = bvnService ?? throw new ArgumentNullException(nameof(bvnService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(OnboardingRequestedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing onboarding request: {RequestReference}", notification.RequestReference);

            var request = await _unitOfWork.OnboardingRequests.GetByIdAsync(notification.OnboardingId, cancellationToken);
            if (request == null)
            {
                _logger.LogWarning("Onboarding request not found: {OnboardingId}", notification.OnboardingId);
                return;
            }

            // Mark as pending BVN verification
            request.MarkBvnVerificationPending();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Skip if not BVN identity type
            if (notification.IdentityNumber is not AfriPay.CORE.ValueObjects.BVN bvn)
            {
                _logger.LogInformation("Skipping BVN verification - identity type is {IdentityType}",
                    notification.IdentityNumber.GetType().Name);
                return;
            }

            // Call BVN verification service
            var verificationResult = await _bvnService.VerifyBvnAsync(
                bvn.Value,
                notification.FirstName,
                notification.LastName,
                cancellationToken);

            if (verificationResult.IsSuccess && verificationResult.Value.IsVerified)
            {
                request.MarkBvnVerified();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("BVN verified for onboarding: {RequestReference}", notification.RequestReference);
            }
            else
            {
                request.MarkBvnVerificationFailed(verificationResult.Error ?? "BVN verification failed");
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("BVN verification failed for: {RequestReference}", notification.RequestReference);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing onboarding request: {RequestReference}", notification.RequestReference);
        }
    }
}
