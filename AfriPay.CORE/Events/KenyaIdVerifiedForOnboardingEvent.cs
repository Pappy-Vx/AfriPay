using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events
{
    /// <summary>
    /// Domain event raised when a Kenya National ID has been successfully verified during onboarding
    /// </summary>
    public record KenyaIdVerifiedForOnboardingEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public Guid OnboardingId { get; init; }
        public KenyaNationalID KenyaNationalId { get; init; }

        public KenyaIdVerifiedForOnboardingEvent(Guid onboardingId, KenyaNationalID kenyaNationalId)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OnboardingId = onboardingId;
            KenyaNationalId = kenyaNationalId;
        }
    }
}
