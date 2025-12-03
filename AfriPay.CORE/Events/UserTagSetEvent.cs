 using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Events;

public record UserTagSetEvent : IDomainEvent
{
  public Guid EventId { get; init; } = Guid.NewGuid();
  public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
  public CustomerId CustomerId { get; init; }
  public UserTag UserTag { get; init; }

  public UserTagSetEvent(CustomerId customerId, UserTag userTag)
  {
    CustomerId = customerId;
    UserTag = userTag;
  }
}