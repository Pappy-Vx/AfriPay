using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.DAL.Repositories
{
   
public class AccountRepository : IAccountRepository
    {
        private readonly AfriPayDbContext _context;
        public AccountRepository(AfriPayDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Account?> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
        }
        public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            var accNum = AccountNumber.Create(accountNumber);
            return await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accNum, cancellationToken);
        }
        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }
        public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddAsync(account, cancellationToken);
        }
        public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Update(account);
            return Task.CompletedTask;
        }
        public async Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
        {
            var accNum = AccountNumber.Create(accountNumber);
            return await _context.Accounts.AnyAsync(a => a.AccountNumber == accNum, cancellationToken);
        }
    }




    //public class AccountRepository : IAccountRepository
    //{
    //    private readonly AfriPayDbContext _context;

    //    public AccountRepository(AfriPayDbContext context)
    //    {
    //        _context = context ?? throw new ArgumentNullException(nameof(context));
    //    }

    //    public async Task<Account?> GetByIdAsync(AccountId accountId, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Accounts
    //            .Include(a => a.Customer)
    //            .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
    //    }

    //    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    //    {
    //        // Use EF.Property to access the converted value directly
    //        return await _context.Accounts
    //            .Include(a => a.Customer)
    //            .FirstOrDefaultAsync(a => EF.Property<string>(a, "AccountNumber") == accountNumber, cancellationToken);
    //    }

    //    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Accounts
    //            .Where(a => a.CustomerId == customerId)
    //            .ToListAsync(cancellationToken);
    //    }

    //    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    //    {
    //        await _context.Accounts.AddAsync(account, cancellationToken);
    //    }

    //    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    //    {
    //        _context.Accounts.Update(account);
    //        return Task.CompletedTask;
    //    }

    //    public async Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    //    {
    //        // Use EF.Property to access the converted value directly
    //        return await _context.Accounts.AnyAsync(a => EF.Property<string>(a, "AccountNumber") == accountNumber, cancellationToken);
    //    }
    //}


}
