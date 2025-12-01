using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record OnboardingRequestedEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public Guid OnboardingId { get; init; }
        public string RequestReference { get; init; }
        public IdentityNumber IdentityNumber { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }

        public OnboardingRequestedEvent(
            Guid onboardingId,
            string requestReference,
            IdentityNumber identityNumber,
            string firstName,
            string lastName,
            string email)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OnboardingId = onboardingId;
            RequestReference = requestReference;
            IdentityNumber = identityNumber;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

}
