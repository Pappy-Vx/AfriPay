using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Onboarding.Commands.StartOnboarding
{
    /// <summary>
    /// Command to initiate the onboarding process for a new customer
    /// </summary>
    public record StartOnboardingCommand : IRequest<Result<StartOnboardingResponse>>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string BVN { get; init; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for onboarding initiation
    /// </summary>
    public record StartOnboardingResponse
    {
        public Guid OnboardingId { get; init; }
        public string RequestReference { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public DateTime RequestedAt { get; init; }
    }
}
