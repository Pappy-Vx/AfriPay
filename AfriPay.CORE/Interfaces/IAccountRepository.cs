using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default);
        Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
        Task AddAsync(Account account, CancellationToken cancellationToken = default);
        Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
        Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    }
}
