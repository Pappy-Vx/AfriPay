using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events;

public record TransferCompletedEvent(
    TransferId TransferId,
    string TransferReference,
    AccountId SourceAccountId,
    AccountId DestinationAccountId,
    CustomerId SourceCustomerId,
    CustomerId DestinationCustomerId,
    decimal Amount,
    decimal TotalDebitAmount,
    string Currency
) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}