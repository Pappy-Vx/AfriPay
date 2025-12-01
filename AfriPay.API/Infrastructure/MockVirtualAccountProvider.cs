using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    public class MockVirtualAccountProvider : IVirtualAccountProvider
    {
        private readonly ILogger<MockVirtualAccountProvider> _logger;
        private static int _accountCounter = 1000000000;

        public MockVirtualAccountProvider(ILogger<MockVirtualAccountProvider> logger)
        {
            _logger = logger;
        }

        public async Task<Result<VirtualAccountResponse>> CreateVirtualAccountAsync(
            string customerReference,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock VA creation for: {CustomerReference}", customerReference);

            // Simulate API call delay
            await Task.Delay(1500, cancellationToken);

            // Generate mock account number
            var accountNumber = Interlocked.Increment(ref _accountCounter).ToString();

            var response = new VirtualAccountResponse
            {
                AccountNumber = accountNumber,
                AccountName = $"{firstName} {lastName}",
                BankName = "AfriPay Virtual Bank",
                ProviderReference = $"VA-{Guid.NewGuid():N}",
                IsSuccess = true
            };

            return Result.Success(response);
        }
    }

}
