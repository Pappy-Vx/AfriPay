using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record OnboardingCompletedEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public Guid OnboardingId { get; init; }
        public CustomerId CustomerId { get; init; }
        public AccountId VirtualAccountId { get; init; }
        public string RequestReference { get; init; }

        public OnboardingCompletedEvent(
            Guid onboardingId,
            CustomerId customerId,
            AccountId virtualAccountId,
            string requestReference)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OnboardingId = onboardingId;
            CustomerId = customerId;
            VirtualAccountId = virtualAccountId;
            RequestReference = requestReference;
        }
    }
}
