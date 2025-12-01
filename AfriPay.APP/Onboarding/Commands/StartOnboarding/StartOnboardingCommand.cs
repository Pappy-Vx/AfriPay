using AfriPay.APP.Common.Models;
using AfriPay.CORE.Enums;
using MediatR;

namespace AfriPay.APP.Onboarding.Commands.StartOnboarding
{
    /// <summary>
    /// Command to initiate the onboarding process for a new customer
    /// </summary>
    public class StartOnboardingCommand : IRequest<Result<StartOnboardingResponse>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public IdentityType IdentityType { get; set; }
        public Country Country { get; set; }
        public string SelfieUrl { get; set; } = string.Empty;
        public bool ConsentGiven { get; set; }
    }

    /// <summary>
    /// Response DTO for onboarding initiation
    /// </summary>
    public class StartOnboardingResponse
    {
        public string RequestId { get; set; } = string.Empty;
        public Guid OnboardingId { get; set; }
        public OnboardingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
