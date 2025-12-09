using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AfriPay.DAL.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly AfriPayDbContext _context;

    public TransferRepository(AfriPayDbContext context)
    {
        _context = context;
    }

    public async Task<Transfer?> GetByIdAsync(TransferId id, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .AsTracking() // Explicitly enable tracking for write operations
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Transfer?> GetByReferenceAsync(string transferReference, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .FirstOrDefaultAsync(t => t.TransferReference == transferReference, cancellationToken);
    }

    public async Task<Transfer?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<List<Transfer>> GetByCustomerIdAsync(CustomerId customerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .Where(t => t.SourceCustomerId == customerId || t.DestinationCustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        await _context.Transfers.AddAsync(transfer, cancellationToken);
    }

    public void Update(Transfer transfer)
    {
        _context.Transfers.Update(transfer);
    }
}