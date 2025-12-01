using AfriPay.CORE.Common;
using AfriPay.APP.Onboarding.Queries.GetOnboardingStatus;
using MediatR;

namespace AfriPay.APP.Onboarding.Queries.GetOnboardingById
{
    /// <summary>
    /// Query to get onboarding request details by ID
    /// This is an alias for GetOnboardingStatusQuery for clarity
    /// </summary>
    public record GetOnboardingByIdQuery : IRequest<Result<OnboardingStatusResponse>>
    {
        public Guid OnboardingId { get; init; }
    }
}
