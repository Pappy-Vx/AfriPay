namespace AfriPay.CORE.Enums
{
    /// <summary>
    /// Manual review case status
    /// </summary>
    public enum ReviewStatus
    {
        Pending = 1,
        InProgress = 2,
        Approved = 3,
        Rejected = 4,
        RequiresMoreInfo = 5
    }

    /// <summary>
    /// Manual review case priority level
    /// </summary>
    public enum ReviewPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
