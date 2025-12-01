using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;

namespace AfriPay.CORE.Interfaces.Services
{
    /// <summary>
    /// Service for core banking operations
    /// </summary>
    public interface ICoreBankingService
    {
        Task<CoreBankingResult> CreateCustomerAsync(
            OnboardingRequest onboardingRequest,
            IdentityVerification identityVerification,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of core banking operations
    /// </summary>
    public class CoreBankingResult
    {
        public bool IsSuccess { get; set; }
        public string? CustomerReference { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
