using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Onboarding.Queries.GetOnboardingStatus
{
    /// <summary>
    /// Query to get the status of an onboarding request
    /// </summary>
    public record GetOnboardingStatusQuery : IRequest<Result<OnboardingStatusResponse>>
    {
        public Guid OnboardingId { get; init; }
    }

    /// <summary>
    /// Response DTO for onboarding status
    /// </summary>
    public record OnboardingStatusResponse
    {
        public Guid OnboardingId { get; init; }
        public string RequestReference { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public Guid? CustomerId { get; init; }
        public Guid? VirtualAccountId { get; init; }
        public DateTime RequestedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public string? FailureReason { get; init; }
    }
}
