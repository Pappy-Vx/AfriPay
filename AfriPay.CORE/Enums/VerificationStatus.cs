namespace AfriPay.CORE.Enums
{
    /// <summary>
    /// Identity verification status
    /// </summary>
    public enum VerificationStatus
    {
        Pending = 1,
        InProgress = 2,
        Verified = 3,
        Failed = 4,
        RequiresManualReview = 5
    }

    /// <summary>
    /// AML screening status
    /// </summary>
    public enum AmlStatus
    {
        Pending = 1,
        InProgress = 2,
        Cleared = 3,
        Flagged = 4,
        RequiresManualReview = 5
    }

    /// <summary>
    /// Risk level for AML screening
    /// </summary>
    public enum RiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
