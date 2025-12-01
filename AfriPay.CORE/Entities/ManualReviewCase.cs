using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;

namespace AfriPay.CORE.Entities
{
    /// <summary>
    /// Entity representing a manual review case for onboarding
    /// </summary>
    public class ManualReviewCase : AuditableEntity
    {
        public Guid OnboardingId { get; private set; }
        public ReviewStatus Status { get; private set; }
        public ReviewPriority Priority { get; private set; }
        public string? AssignedTo { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public string? Resolution { get; private set; }
        public string? ResolutionNotes { get; private set; }
        public string Reason { get; private set; }
        public List<string> Comments { get; private set; }

        private ManualReviewCase()
        {
            Comments = new List<string>();
        }

        private ManualReviewCase(
            Guid onboardingId,
            string reason,
            ReviewPriority priority)
        {
            OnboardingId = onboardingId;
            Reason = reason;
            Priority = priority;
            Status = ReviewStatus.Pending;
            Comments = new List<string>();
        }

        public static ManualReviewCase Create(
            Guid onboardingId,
            string reason,
            ReviewPriority priority = ReviewPriority.Medium)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason cannot be empty", nameof(reason));

            return new ManualReviewCase(onboardingId, reason, priority);
        }

        public void AssignTo(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            AssignedTo = userId;
            AssignedAt = DateTime.UtcNow;
            Status = ReviewStatus.InProgress;
        }

        public void Approve(string resolutionNotes, string userId)
        {
            Status = ReviewStatus.Approved;
            Resolution = "Approved";
            ResolutionNotes = resolutionNotes;
            ResolvedAt = DateTime.UtcNow;
            SetUpdatedBy(userId);
        }

        public void Reject(string resolutionNotes, string userId)
        {
            Status = ReviewStatus.Rejected;
            Resolution = "Rejected";
            ResolutionNotes = resolutionNotes;
            ResolvedAt = DateTime.UtcNow;
            SetUpdatedBy(userId);
        }

        public void RequestMoreInfo(string comment)
        {
            Status = ReviewStatus.RequiresMoreInfo;
            AddComment(comment);
        }

        public void AddComment(string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                Comments.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {comment}");
            }
        }

        public void UpdatePriority(ReviewPriority newPriority)
        {
            Priority = newPriority;
        }
    }
}
