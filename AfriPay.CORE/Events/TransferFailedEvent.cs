using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events;

public record TransferFailedEvent(
    TransferId TransferId,
    string TransferReference,
    CustomerId SourceCustomerId,
    string Reason
) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}