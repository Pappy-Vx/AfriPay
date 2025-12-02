using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace AfriPay.CORE.Common
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
        Guid EventId { get; }
    }
}
