using AfriPay.CORE.ValueObjects;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Interfaces.Services
{
    /// <summary>
    /// Service for identity verification
    /// </summary>
    public interface IIdentityVerificationService
    {
        Task<IdentityVerificationResult> VerifyAsync(
            IdentityNumber identityNumber,
            PersonalInfo personalInfo,
            string selfieUrl,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of identity verification
    /// </summary>
    public class IdentityVerificationResult
    {
        public bool IsVerified { get; set; }
        public string? VerifiedFirstName { get; set; }
        public string? VerifiedLastName { get; set; }
        public string? VerifiedMiddleName { get; set; }
        public DateTime? VerifiedDateOfBirth { get; set; }
        public string? VerifiedPhone { get; set; }
        public double NameMatchScore { get; set; }
        public decimal PhotoMatchScore { get; set; }
        public string? ProviderReference { get; set; }
        public string? ProviderResponse { get; set; }
        public string? FailureReason { get; set; }
    }
}
