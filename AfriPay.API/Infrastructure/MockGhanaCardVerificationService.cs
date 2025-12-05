using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    /// <summary>
    /// Mock implementation of Ghana Card verification service for development/testing
    /// </summary>
    public class MockGhanaCardVerificationService : IGhanaCardVerificationService
    {
        private readonly ILogger<MockGhanaCardVerificationService> _logger;

        public MockGhanaCardVerificationService(ILogger<MockGhanaCardVerificationService> logger)
        {
            _logger = logger;
        }

        public async Task<Result<GhanaCardVerificationResponse>> VerifyGhanaCardAsync(
            string ghanaCardNumber,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock Ghana Card verification for: {GhanaCardNumber}", ghanaCardNumber);

            // Simulate API delay
            await Task.Delay(500, cancellationToken);

            // Mock verification logic - in production, this would call actual Ghana Card API
            // Expected format: GHA-XXXXXXXXX-X
            if (!System.Text.RegularExpressions.Regex.IsMatch(ghanaCardNumber, @"^GHA-\d{9}-\d{1}$"))
            {
                return Result.Failure<GhanaCardVerificationResponse>("Invalid Ghana Card format. Expected format: GHA-XXXXXXXXX-X");
            }

            // For demo purposes, all valid format Ghana Cards pass verification
            var response = new GhanaCardVerificationResponse
            {
                IsVerified = true,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = "1990-01-01",
                PhoneNumber = "+233123456789"
            };

            _logger.LogInformation("Ghana Card verification successful for: {GhanaCardNumber}", ghanaCardNumber);

            return Result.Success(response);
        }
    }
}
