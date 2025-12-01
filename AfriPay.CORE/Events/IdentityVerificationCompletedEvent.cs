using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;

namespace AfriPay.CORE.Events
{
    /// <summary>
    /// Event raised when identity verification is completed
    /// </summary>
    public class IdentityVerificationCompletedEvent : DomainEvent
    {
        public Guid OnboardingId { get; }
        public Guid VerificationId { get; }
        public VerificationStatus Status { get; }
        public bool IsSuccessful { get; }
        public string? FailureReason { get; }

        public IdentityVerificationCompletedEvent(
            Guid onboardingId,
            Guid verificationId,
            VerificationStatus status,
            bool isSuccessful,
            string? failureReason = null)
        {
            OnboardingId = onboardingId;
            VerificationId = verificationId;
            Status = status;
            IsSuccessful = isSuccessful;
            FailureReason = failureReason;
        }
    }
}
