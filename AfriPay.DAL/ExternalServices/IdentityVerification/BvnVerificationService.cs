using AfriPay.CORE.Interfaces.Services;
using AfriPay.CORE.ValueObjects;
using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AfriPay.DAL.ExternalServices.IdentityVerification
{
    public class BvnVerificationService : IIdentityVerificationService
    {
        private readonly ILogger<BvnVerificationService> _logger;

        public BvnVerificationService(ILogger<BvnVerificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IdentityVerificationResult> VerifyAsync(
            IdentityNumber identityNumber,
            PersonalInfo personalInfo,
            string selfieUrl,
            CancellationToken cancellationToken = default)
        {
            if (identityNumber is not BVN bvn)
                throw new ArgumentException("Expected BVN number", nameof(identityNumber));

            try
            {
                _logger.LogInformation(
                    "Calling BVN provider for verification - BVN: {BVN}, Name: {FirstName} {LastName}",
                    bvn.Value.Substring(0, 3) + "***", // Mask BVN
                    personalInfo.FirstName,
                    personalInfo.LastName);

                // Simulate external API call
                await Task.Delay(1000, cancellationToken);

                // Mock verification logic - in production, call actual BVN provider
                var isVerified = await MockBvnVerification(bvn, personalInfo, selfieUrl);

                if (!isVerified)
                {
                    _logger.LogWarning("BVN verification failed for {BVN}", bvn.Value.Substring(0, 3) + "***");
                    return new IdentityVerificationResult
                    {
                        IsVerified = false,
                        FailureReason = "BVN verification failed - details mismatch"
                    };
                }

                // Calculate match scores
                var nameMatchScore = CalculateNameMatch(
                    personalInfo.FirstName,
                    personalInfo.LastName,
                    personalInfo.FirstName, // Mock: same as input
                    personalInfo.LastName);

                _logger.LogInformation(
                    "BVN verification successful with match score: {Score}",
                    nameMatchScore);

                return new IdentityVerificationResult
                {
                    IsVerified = nameMatchScore >= 0.8, // 80% threshold
                    VerifiedFirstName = personalInfo.FirstName,
                    VerifiedLastName = personalInfo.LastName,
                    VerifiedMiddleName = personalInfo.MiddleName,
                    VerifiedDateOfBirth = personalInfo.DateOfBirth,
                    VerifiedPhone = null, // Would come from provider
                    NameMatchScore = nameMatchScore,
                    PhotoMatchScore = 0.95m, // Mock photo match
                    ProviderReference = $"BVN-{Guid.NewGuid():N}",
                    ProviderResponse = "Mock BVN verification successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BVN provider");
                throw;
            }
        }

        private async Task<bool> MockBvnVerification(BVN bvn, PersonalInfo personalInfo, string selfieUrl)
        {
            // Mock implementation - always return true for valid BVNs
            await Task.CompletedTask;
            return bvn.Value.Length == 11;
        }

        private double CalculateNameMatch(
            string firstName1,
            string lastName1,
            string firstName2,
            string lastName2)
        {
            // Simplified - use proper fuzzy matching library like FuzzySharp in production
            var firstNameMatch = string.Equals(
                firstName1,
                firstName2,
                StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;

            var lastNameMatch = string.Equals(
                lastName1,
                lastName2,
                StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;

            return (firstNameMatch + lastNameMatch) / 2.0;
        }
    }
}
