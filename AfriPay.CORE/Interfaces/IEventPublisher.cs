using AfriPay.CORE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent;

        Task PublishManyAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}
