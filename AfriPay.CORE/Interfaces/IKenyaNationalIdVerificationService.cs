using AfriPay.CORE.Common;

namespace AfriPay.CORE.Interfaces
{
    /// <summary>
    /// Service for verifying Kenya National ID documents
    /// </summary>
    public interface IKenyaNationalIdVerificationService
    {
        Task<Result<KenyaNationalIdVerificationResponse>> VerifyKenyaNationalIdAsync(
            string nationalIdNumber,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default);
    }

    public class KenyaNationalIdVerificationResponse
    {
        public bool IsVerified { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }
}
