using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Onboarding.Queries.GetOnboardingStatus
{
    /// <summary>
    /// Handler for GetOnboardingStatusQuery
    /// </summary>
    public class GetOnboardingStatusQueryHandler : IRequestHandler<GetOnboardingStatusQuery, Result<OnboardingStatusResponse>>
    {
        private readonly IOnboardingRequestRepository _onboardingRepository;
        private readonly ILogger<GetOnboardingStatusQueryHandler> _logger;

        public GetOnboardingStatusQueryHandler(
            IOnboardingRequestRepository onboardingRepository,
            ILogger<GetOnboardingStatusQueryHandler> logger)
        {
            _onboardingRepository = onboardingRepository ?? throw new ArgumentNullException(nameof(onboardingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<OnboardingStatusResponse>> Handle(
            GetOnboardingStatusQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving onboarding status for {OnboardingId}",
                    request.OnboardingId);

                var onboardingRequest = await _onboardingRepository.GetByIdAsync(request.OnboardingId, cancellationToken);

                if (onboardingRequest == null)
                {
                    _logger.LogWarning(
                        "Onboarding request not found: {OnboardingId}",
                        request.OnboardingId);
                    return Result.Failure<OnboardingStatusResponse>("Onboarding request not found");
                }

                var response = new OnboardingStatusResponse
                {
                    OnboardingId = onboardingRequest.OnboardingId,
                    RequestReference = onboardingRequest.RequestReference,
                    Status = onboardingRequest.Status.ToString(),
                    Message = GetStatusMessage(onboardingRequest.Status),
                    CustomerId = onboardingRequest.CustomerId?.Value,
                    VirtualAccountId = onboardingRequest.VirtualAccountId?.Value,
                    RequestedAt = onboardingRequest.RequestedAt,
                    CompletedAt = onboardingRequest.CompletedAt,
                    FailureReason = onboardingRequest.FailureReason
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving onboarding status for {OnboardingId}",
                    request.OnboardingId);
                return Result.Failure<OnboardingStatusResponse>("An error occurred while retrieving the onboarding status");
            }
        }

        private static string GetStatusMessage(OnboardingStatus status)
        {
            return status switch
            {
                OnboardingStatus.Initiated => "Onboarding request initiated",
                OnboardingStatus.BvnVerificationPending => "BVN verification in progress",
                OnboardingStatus.BvnVerified => "BVN verification successful",
                OnboardingStatus.BvnVerificationFailed => "BVN verification failed",
                OnboardingStatus.CustomerCreated => "Customer account created",
                OnboardingStatus.VirtualAccountCreationPending => "Virtual account creation in progress",
                OnboardingStatus.VirtualAccountCreated => "Virtual account created successfully",
                OnboardingStatus.VirtualAccountCreationFailed => "Virtual account creation failed",
                OnboardingStatus.Completed => "Onboarding completed successfully",
                OnboardingStatus.Failed => "Onboarding failed",
                _ => "Unknown status"
            };
        }
    }
}
