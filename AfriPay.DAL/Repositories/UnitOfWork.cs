using AfriPay.CORE.Interfaces;
using AfriPay.DAL.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AfriPayDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AfriPayDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OnboardingRequests = new OnboardingRequestRepository(_context);
            Customers = new CustomerRepository(_context);
            Accounts = new AccountRepository(_context);
        }

        public IOnboardingRequestRepository OnboardingRequests { get; }
        public ICustomerRepository Customers { get; }
        public IAccountRepository Accounts { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
