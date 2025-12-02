using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    public class MockVirtualAccountProvider : IVirtualAccountProvider
    {
        private readonly ILogger<MockVirtualAccountProvider> _logger;

        public MockVirtualAccountProvider(ILogger<MockVirtualAccountProvider> logger)
        {
            _logger = logger;
        }

        public Task<Result<VirtualAccountResponse>> CreateVirtualAccountAsync(
            string customerReference,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            var customerName = $"{firstName} {lastName}";
            _logger.LogInformation("Creating mock virtual account for: {CustomerName}, Ref: {CustomerReference}",
                customerName, customerReference);

            // Generate a mock account number (10 digits starting with 10)
            var accountNumber = $"10{Random.Shared.Next(10000000, 99999999)}";

            var response = new VirtualAccountResponse
            {
                AccountNumber = accountNumber,
                AccountName = customerName,
                BankName = "AfriPay Mock Bank",
                ProviderReference = $"MOCK-{Guid.NewGuid():N}"[..20].ToUpper(),
                IsSuccess = true
            };

            _logger.LogInformation("Mock virtual account created: {AccountNumber}", accountNumber);

            return Task.FromResult(Result<VirtualAccountResponse>.Success(response));
        }
    }
}
