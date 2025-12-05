using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    /// <summary>
    /// Mock implementation of Kenya National ID verification service for development/testing
    /// </summary>
    public class MockKenyaNationalIdVerificationService : IKenyaNationalIdVerificationService
    {
        private readonly ILogger<MockKenyaNationalIdVerificationService> _logger;

        public MockKenyaNationalIdVerificationService(ILogger<MockKenyaNationalIdVerificationService> logger)
        {
            _logger = logger;
        }

        public async Task<Result<KenyaNationalIdVerificationResponse>> VerifyKenyaNationalIdAsync(
            string nationalIdNumber,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock Kenya National ID verification for: {NationalId}", nationalIdNumber);

            // Simulate API delay
            await Task.Delay(500, cancellationToken);

            // Mock verification logic - in production, this would call actual Kenya National ID API
            // Expected format: 7-9 digits
            if (nationalIdNumber.Length < 7 || nationalIdNumber.Length > 9)
            {
                return Result.Failure<KenyaNationalIdVerificationResponse>("Invalid Kenya National ID format. Must be 7-9 digits");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(nationalIdNumber, @"^\d+$"))
            {
                return Result.Failure<KenyaNationalIdVerificationResponse>("Invalid Kenya National ID format. Must contain only digits");
            }

            // For demo purposes, all valid format Kenya IDs pass verification
            var response = new KenyaNationalIdVerificationResponse
            {
                IsVerified = true,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = "1990-01-01",
                PhoneNumber = "+254712345678"
            };

            _logger.LogInformation("Kenya National ID verification successful for: {NationalId}", nationalIdNumber);

            return Result.Success(response);
        }
    }
}
