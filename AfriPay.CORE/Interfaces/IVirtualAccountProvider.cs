using AfriPay.CORE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IVirtualAccountProvider
    {
        Task<Result<VirtualAccountResponse>> CreateVirtualAccountAsync(
            string customerReference,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            CancellationToken cancellationToken = default);
    }

    public class VirtualAccountResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string ProviderReference { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

}
