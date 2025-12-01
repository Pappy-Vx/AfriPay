namespace AfriPay.CORE.Common
{
    /// <summary>
    /// Base class for auditable entities with creation and modification tracking
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? UpdatedBy { get; private set; }

        protected AuditableEntity() : base()
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = "System"; // Default value, will be overridden by actual user
        }

        protected AuditableEntity(Guid id) : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = "System";
        }

        /// <summary>
        /// Sets the creation audit information
        /// </summary>
        public void SetCreatedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            CreatedBy = userId;
        }

        /// <summary>
        /// Sets the modification audit information
        /// </summary>
        public void SetUpdatedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userId;
        }
    }
}
