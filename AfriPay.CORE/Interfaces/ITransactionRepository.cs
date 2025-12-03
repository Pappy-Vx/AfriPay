using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Interfaces;

public interface ITransactionRepository
{
  Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<Transaction>> GetByAccountIdAsync(AccountId accountId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
  Task<List<Transaction>> GetByCustomerIdAsync(CustomerId customerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
  Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
  Task AddRangeAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);
}