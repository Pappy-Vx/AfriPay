namespace AfriPay.APP.Transfers.Queries.GetTransferById;

public class TransferDto
{
    public Guid TransferId { get; set; }
    public string TransferReference { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }

    public Guid SourceAccountId { get; set; }
    public Guid SourceCustomerId { get; set; }

    public Guid DestinationAccountId { get; set; }
    public Guid DestinationCustomerId { get; set; }
    public string? DestinationUserTag { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";
    public decimal? Fee { get; set; }
    public decimal TotalDebitAmount { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Narration { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
