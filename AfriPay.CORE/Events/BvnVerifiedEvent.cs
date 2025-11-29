using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record BvnVerifiedEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public CustomerId CustomerId { get; init; }
        public BVN BVN { get; init; }

        public BvnVerifiedEvent(CustomerId customerId, BVN bvn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            CustomerId = customerId;
            BVN = bvn;
        }
    }
}
