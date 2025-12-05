using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events
{
    /// <summary>
    /// Domain event raised when a Ghana Card has been successfully verified during onboarding
    /// </summary>
    public record GhanaCardVerifiedForOnboardingEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public Guid OnboardingId { get; init; }
        public GhanaCard GhanaCard { get; init; }

        public GhanaCardVerifiedForOnboardingEvent(Guid onboardingId, GhanaCard ghanaCard)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OnboardingId = onboardingId;
            GhanaCard = ghanaCard;
        }
    }
}
