using AfriPay.CORE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IBvnVerificationService
    {
        Task<Result<BvnVerificationResponse>> VerifyBvnAsync(
            string bvn,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default);
    }

    public class BvnVerificationResponse
    {
        public bool IsVerified { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }
}
