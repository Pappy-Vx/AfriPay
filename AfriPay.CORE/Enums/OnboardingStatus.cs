using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Enums
{
    public enum OnboardingStatus
    {
        Initiated = 1,
        BvnVerificationPending = 2,
        BvnVerified = 3,
        BvnVerificationFailed = 4,
        CustomerCreated = 5,
        VirtualAccountCreationPending = 6,
        VirtualAccountCreated = 7,
        VirtualAccountCreationFailed = 8,
        Completed = 9,
        Failed = 10
    }
}
