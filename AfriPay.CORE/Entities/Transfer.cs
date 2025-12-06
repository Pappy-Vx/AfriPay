using AfriPay.CORE.Common;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Events;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities;

public class Transfer : AggregateRoot<TransferId>
{
    public string TransferReference { get; private set; } = string.Empty;
    public string? IdempotencyKey { get; private set; }

    // Source
    public AccountId SourceAccountId { get; private set; }
    public CustomerId SourceCustomerId { get; private set; }

    // Destination
    public AccountId DestinationAccountId { get; private set; }
    public CustomerId DestinationCustomerId { get; private set; }
    public string? DestinationUserTag { get; private set; }

    // Amount
    public Money Amount { get; private set; }
    public Money? Fee { get; private set; }
    public Money TotalDebitAmount { get; private set; }

    // Details
    public TransferType Type { get; private set; }
    public TransferStatus Status { get; private set; }
    public string? Description { get; private set; }
    public string? Narration { get; private set; }
    public string? FailureReason { get; private set; }

    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Transfer()
    {
        // EF Core requires parameterless constructor
    }

        public static Transfer Create(
        AccountId sourceAccountId,
        CustomerId sourceCustomerId,
        AccountId destinationAccountId,
        CustomerId destinationCustomerId,
        Money amount,
        Money? fee,
        TransferType type,
        string? description = null,
        string? destinationUserTag = null,
        string? idempotencyKey = null)
    {
        if (sourceAccountId.Value == destinationAccountId.Value)
            throw new InvalidOperationException("Cannot transfer to the same account");

        if (amount.Amount <= 0)
            throw new InvalidOperationException("Transfer amount must be greater than zero");

        // Always use distinct Money instances for Amount and TotalDebitAmount
        // so EF Core does not try to track the same owned entity instance twice.
        var totalDebit = fee != null
            ? new Money(amount.Amount + fee.Amount, amount.Currency)
            : new Money(amount.Amount, amount.Currency);

        var transfer = new Transfer
        {
            Id = TransferId.Create(),
            TransferReference = GenerateReference(),
            IdempotencyKey = idempotencyKey,
            SourceAccountId = sourceAccountId,
            SourceCustomerId = sourceCustomerId,
            DestinationAccountId = destinationAccountId,
            DestinationCustomerId = destinationCustomerId,
            DestinationUserTag = destinationUserTag,
            Amount = new Money(amount.Amount, amount.Currency),
            Fee = fee != null ? new Money(fee.Amount, fee.Currency) : null,
            TotalDebitAmount = totalDebit,
            Type = type,
            Status = TransferStatus.Pending,
            Description = description,
            Narration = GenerateNarration(destinationUserTag),
            CreatedAt = DateTime.UtcNow
        };

        transfer.AddDomainEvent(new TransferInitiatedEvent(
            transfer.Id,
            transfer.TransferReference,
            transfer.SourceCustomerId,
            transfer.DestinationCustomerId));

        return transfer;
    }

    public void MarkAsProcessing()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Cannot process transfer in status {Status}");

        Status = TransferStatus.Processing;
    }

    public void Complete()
    {
        if (Status != TransferStatus.Processing)
            throw new InvalidOperationException($"Cannot complete transfer in status {Status}");

        Status = TransferStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new TransferCompletedEvent(
            Id,
            TransferReference,
            SourceAccountId,
            DestinationAccountId,
            SourceCustomerId,
            DestinationCustomerId,
            Amount.Amount,
            TotalDebitAmount.Amount,
            Amount.Currency));
    }

    /// <summary>
    /// Update transfer monetary values after cross-currency settlement.
    /// Destination amount represents the value to credit in the destination account currency.
    /// Total debit amount represents the full amount to debit from the source account in its currency.
    /// </summary>
    public void UpdateAmounts(Money destinationAmount, Money totalDebitAmount)
    {
        if (destinationAmount is null) throw new ArgumentNullException(nameof(destinationAmount));
        if (totalDebitAmount is null) throw new ArgumentNullException(nameof(totalDebitAmount));

        if (Status != TransferStatus.Pending && Status != TransferStatus.Processing)
            throw new InvalidOperationException($"Cannot update amounts for transfer in status {Status}");

        Amount = destinationAmount;
        TotalDebitAmount = totalDebitAmount;
    }

    public void Fail(string reason)
    {
        Status = TransferStatus.Failed;
        FailureReason = reason;

        AddDomainEvent(new TransferFailedEvent(
            Id,
            TransferReference,
            SourceCustomerId,
            reason));
    }

    private static string GenerateReference()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static string GenerateNarration(string? destinationTag)
    {
        return !string.IsNullOrWhiteSpace(destinationTag)
            ? $"Transfer to {destinationTag}"
            : "Transfer";
    }
}