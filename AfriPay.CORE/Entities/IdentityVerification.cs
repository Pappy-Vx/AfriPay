using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities
{
    /// <summary>
    /// Entity representing identity verification for onboarding
    /// </summary>
    public class IdentityVerification : AuditableEntity
    {
        public Guid OnboardingId { get; private set; }
        public IdentityNumber IdentityNumber { get; private set; }
        public VerificationStatus Status { get; private set; }
        public string? VerificationProvider { get; private set; }
        public string? VerificationReference { get; private set; }
        public DateTime? VerificationDate { get; private set; }
        public decimal? MatchScore { get; private set; }
        public string? FailureReason { get; private set; }
        public string? RawResponse { get; private set; }

        private IdentityVerification() { } // For EF Core

        private IdentityVerification(
            Guid onboardingId,
            IdentityNumber identityNumber)
        {
            OnboardingId = onboardingId;
            IdentityNumber = identityNumber;
            Status = VerificationStatus.Pending;
        }

        public static IdentityVerification Create(
            Guid onboardingId,
            IdentityNumber identityNumber)
        {
            var verification = new IdentityVerification(onboardingId, identityNumber);

            verification.AddDomainEvent(new IdentityVerificationRequestedEvent(
                onboardingId,
                verification.Id,
                identityNumber.Value,
                identityNumber.Country));

            return verification;
        }

        public void MarkAsInProgress(string provider, string reference)
        {
            Status = VerificationStatus.InProgress;
            VerificationProvider = provider;
            VerificationReference = reference;
        }

        public void CompleteVerification(
            bool isSuccessful,
            decimal matchScore,
            string? rawResponse = null,
            string? failureReason = null)
        {
            Status = isSuccessful ? VerificationStatus.Verified : VerificationStatus.Failed;
            VerificationDate = DateTime.UtcNow;
            MatchScore = matchScore;
            RawResponse = rawResponse;
            FailureReason = failureReason;

            AddDomainEvent(new IdentityVerificationCompletedEvent(
                OnboardingId,
                Id,
                Status,
                isSuccessful,
                failureReason));
        }

        public void RequireManualReview(string reason)
        {
            Status = VerificationStatus.RequiresManualReview;
            FailureReason = reason;
        }
    }
}
