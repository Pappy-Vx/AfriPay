using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AfriPay.DAL.Repositories;

public class TransactionRepository : ITransactionRepository
{
  private readonly AfriPayDbContext _context;

  public TransactionRepository(AfriPayDbContext context)
  {
    _context = context;
  }

  public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Transactions
        .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public async Task<List<Transaction>> GetByAccountIdAsync(AccountId accountId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
  {
    return await _context.Transactions
        .Where(t => t.AccountId == accountId)
        .OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<List<Transaction>> GetByCustomerIdAsync(CustomerId customerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
  {
    return await _context.Transactions
        .Where(t => t.CustomerId == customerId)
        .OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
  {
    await _context.Transactions.AddAsync(transaction, cancellationToken);
  }

  public async Task AddRangeAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
  {
    await _context.Transactions.AddRangeAsync(transactions, cancellationToken);
  }
}