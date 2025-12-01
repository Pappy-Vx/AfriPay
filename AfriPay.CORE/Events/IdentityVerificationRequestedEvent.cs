using AfriPay.CORE.Common;

namespace AfriPay.CORE.Events
{
    /// <summary>
    /// Event raised when identity verification is requested
    /// </summary>
    public class IdentityVerificationRequestedEvent : DomainEvent
    {
        public Guid OnboardingId { get; }
        public Guid VerificationId { get; }
        public string IdentityNumber { get; }
        public string Country { get; }

        public IdentityVerificationRequestedEvent(
            Guid onboardingId,
            Guid verificationId,
            string identityNumber,
            string country)
        {
            OnboardingId = onboardingId;
            VerificationId = verificationId;
            IdentityNumber = identityNumber;
            Country = country;
        }
    }
}
