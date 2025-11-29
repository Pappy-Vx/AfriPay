using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record VirtualAccountCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public AccountId AccountId { get; init; }
        public AccountNumber AccountNumber { get; init; }
        public CustomerId CustomerId { get; init; }
        public string ProviderReference { get; init; }

        public VirtualAccountCreatedEvent(
            AccountId accountId,
            AccountNumber accountNumber,
            CustomerId customerId,
            string providerReference)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            AccountId = accountId;
            AccountNumber = accountNumber;
            CustomerId = customerId;
            ProviderReference = providerReference;
        }
    }

}
