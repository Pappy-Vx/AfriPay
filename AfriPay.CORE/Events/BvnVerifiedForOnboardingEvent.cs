using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Events
{
    public record BvnVerifiedForOnboardingEvent : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTime OccurredOn { get; init; }
        public Guid OnboardingId { get; init; }
        public BVN BVN { get; init; }

        public BvnVerifiedForOnboardingEvent(Guid onboardingId, BVN bvn)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OnboardingId = onboardingId;
            BVN = bvn;
        }
    }
}
