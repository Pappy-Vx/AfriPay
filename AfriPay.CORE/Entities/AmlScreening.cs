using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities
{
    /// <summary>
    /// Entity representing AML (Anti-Money Laundering) screening
    /// </summary>
    public class AmlScreening : AuditableEntity
    {
        public Guid OnboardingId { get; private set; }
        public AmlStatus Status { get; private set; }
        public RiskLevel RiskLevel { get; private set; }
        public decimal RiskScore { get; private set; }
        public string? ScreeningProvider { get; private set; }
        public string? ScreeningReference { get; private set; }
        public DateTime? ScreeningDate { get; private set; }
        public List<string> Flags { get; private set; }
        public string? Notes { get; private set; }
        public string? RawResponse { get; private set; }

        private AmlScreening()
        {
            Flags = new List<string>();
        }

        private AmlScreening(Guid onboardingId)
        {
            OnboardingId = onboardingId;
            Status = AmlStatus.Pending;
            RiskLevel = RiskLevel.Low;
            RiskScore = 0;
            Flags = new List<string>();
        }

        public static AmlScreening Create(Guid onboardingId)
        {
            return new AmlScreening(onboardingId);
        }

        public void MarkAsInProgress(string provider, string reference)
        {
            Status = AmlStatus.InProgress;
            ScreeningProvider = provider;
            ScreeningReference = reference;
        }

        public void CompleteScreening(
            bool isCleared,
            RiskLevel riskLevel,
            decimal riskScore,
            List<string> flags,
            string? rawResponse = null,
            string? notes = null)
        {
            Status = isCleared ? AmlStatus.Cleared : AmlStatus.Flagged;
            RiskLevel = riskLevel;
            RiskScore = riskScore;
            Flags = flags ?? new List<string>();
            ScreeningDate = DateTime.UtcNow;
            RawResponse = rawResponse;
            Notes = notes;
        }

        public void RequireManualReview(string reason)
        {
            Status = AmlStatus.RequiresManualReview;
            Notes = reason;
        }

        public void AddFlag(string flag)
        {
            if (!Flags.Contains(flag))
            {
                Flags.Add(flag);
            }
        }
    }
}
