using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class OnboardingRequestedHandler : INotificationHandler<OnboardingRequestedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBvnVerificationService _bvnService;
    private readonly IGhanaCardVerificationService _ghanaCardService;
    private readonly IKenyaNationalIdVerificationService _kenyaIdService;
    private readonly ILogger<OnboardingRequestedHandler> _logger;

    public OnboardingRequestedHandler(
        IUnitOfWork unitOfWork,
        IBvnVerificationService bvnService,
        IGhanaCardVerificationService ghanaCardService,
        IKenyaNationalIdVerificationService kenyaIdService,
        ILogger<OnboardingRequestedHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _bvnService = bvnService ?? throw new ArgumentNullException(nameof(bvnService));
        _ghanaCardService = ghanaCardService ?? throw new ArgumentNullException(nameof(ghanaCardService));
        _kenyaIdService = kenyaIdService ?? throw new ArgumentNullException(nameof(kenyaIdService));
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

            // Route to appropriate verification service based on identity type
            switch (notification.IdentityNumber)
            {
                case BVN bvn:
                    await HandleBvnVerification(request, bvn, notification, cancellationToken);
                    break;

                case GhanaCard ghanaCard:
                    await HandleGhanaCardVerification(request, ghanaCard, notification, cancellationToken);
                    break;

                case KenyaNationalID kenyaId:
                    await HandleKenyaIdVerification(request, kenyaId, notification, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unsupported identity type: {IdentityType}", notification.IdentityNumber.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing onboarding request: {RequestReference}", notification.RequestReference);
        }
    }

    private async Task HandleBvnVerification(
        CORE.Entities.OnboardingRequest request,
        BVN bvn,
        OnboardingRequestedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting BVN verification for: {RequestReference}", notification.RequestReference);

        request.MarkBvnVerificationPending();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

    private async Task HandleGhanaCardVerification(
        CORE.Entities.OnboardingRequest request,
        GhanaCard ghanaCard,
        OnboardingRequestedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Ghana Card verification for: {RequestReference}", notification.RequestReference);

        request.MarkGhanaCardVerificationPending();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var verificationResult = await _ghanaCardService.VerifyGhanaCardAsync(
            ghanaCard.Value,
            notification.FirstName,
            notification.LastName,
            cancellationToken);

        if (verificationResult.IsSuccess && verificationResult.Value.IsVerified)
        {
            request.MarkGhanaCardVerified();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Ghana Card verified for onboarding: {RequestReference}", notification.RequestReference);
        }
        else
        {
            request.MarkGhanaCardVerificationFailed(verificationResult.Error ?? "Ghana Card verification failed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Ghana Card verification failed for: {RequestReference}", notification.RequestReference);
        }
    }

    private async Task HandleKenyaIdVerification(
        CORE.Entities.OnboardingRequest request,
        KenyaNationalID kenyaId,
        OnboardingRequestedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Kenya National ID verification for: {RequestReference}", notification.RequestReference);

        request.MarkKenyaIdVerificationPending();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var verificationResult = await _kenyaIdService.VerifyKenyaNationalIdAsync(
            kenyaId.Value,
            notification.FirstName,
            notification.LastName,
            cancellationToken);

        if (verificationResult.IsSuccess && verificationResult.Value.IsVerified)
        {
            request.MarkKenyaIdVerified();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Kenya National ID verified for onboarding: {RequestReference}", notification.RequestReference);
        }
        else
        {
            request.MarkKenyaIdVerificationFailed(verificationResult.Error ?? "Kenya National ID verification failed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Kenya National ID verification failed for: {RequestReference}", notification.RequestReference);
        }
    }
}
