using AfriPay.CORE.Common;

namespace AfriPay.CORE.Interfaces
{
    /// <summary>
    /// Service for verifying Ghana Card identity documents
    /// </summary>
    public interface IGhanaCardVerificationService
    {
        Task<Result<GhanaCardVerificationResponse>> VerifyGhanaCardAsync(
            string ghanaCardNumber,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default);
    }

    public class GhanaCardVerificationResponse
    {
        public bool IsVerified { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }
}
