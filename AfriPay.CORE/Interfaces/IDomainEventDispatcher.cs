using AfriPay.CORE.Common;

namespace AfriPay.CORE.Interfaces
{
    /// <summary>
    /// Interface for dispatching domain events
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
