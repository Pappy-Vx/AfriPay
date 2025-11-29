using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record CustomerCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public CustomerId CustomerId { get; init; }
        public CustomerReference CustomerReference { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }

        public CustomerCreatedEvent(
            CustomerId customerId,
            CustomerReference customerReference,
            string firstName,
            string lastName,
            string email)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            CustomerId = customerId;
            CustomerReference = customerReference;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
