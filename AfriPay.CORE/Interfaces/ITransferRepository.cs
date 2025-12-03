using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Interfaces;

public interface ITransferRepository
{
  Task<Transfer?> GetByIdAsync(TransferId id, CancellationToken cancellationToken = default);
  Task<Transfer?> GetByReferenceAsync(string transferReference, CancellationToken cancellationToken = default);
  Task<Transfer?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
  Task<List<Transfer>> GetByCustomerIdAsync(CustomerId customerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
  Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
  void Update(Transfer transfer);
}