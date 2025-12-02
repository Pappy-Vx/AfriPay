using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AfriPay.CORE.Events;

public class OnboardingRequestedEvent : DomainEvent
{
    public Guid OnboardingId { get; }
    public string RequestReference { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public IdentityNumber IdentityNumber { get; }

    public OnboardingRequestedEvent(
        Guid onboardingId,
        string requestReference,
        string firstName,
        string lastName,
        IdentityNumber identityNumber)
    {
        OnboardingId = onboardingId;
        RequestReference = requestReference;
        FirstName = firstName;
        LastName = lastName;
        IdentityNumber = identityNumber;
    }
}