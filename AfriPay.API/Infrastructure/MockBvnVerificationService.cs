using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    public class MockBvnVerificationService : IBvnVerificationService
    {
        private readonly ILogger<MockBvnVerificationService> _logger;

        public MockBvnVerificationService(ILogger<MockBvnVerificationService> logger)
        {
            _logger = logger;
        }

        public async Task<Result<BvnVerificationResponse>> VerifyBvnAsync(
            string bvn,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock BVN verification for: {BVN}", bvn);

            // Mock verification logic - in production, this would call actual BVN API
            if (bvn.Length != 11)
            {
                return Result.Failure<BvnVerificationResponse>("Invalid BVN format");
            }

            // For demo purposes, all valid format BVNs pass verification
            var response = new BvnVerificationResponse
            {
                IsVerified = true,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = "1990-01-01",
                PhoneNumber = "08012345678"
            };

            return Result.Success(response);
        }
    }

}
