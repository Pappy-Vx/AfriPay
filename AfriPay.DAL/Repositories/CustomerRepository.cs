using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace AfriPay.DAL.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AfriPayDbContext _context;
        public CustomerRepository(AfriPayDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AsTracking() // Explicitly enable tracking for write operations
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }
        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.ContactInfo!.Email == email, cancellationToken);
        }
        public async Task<Customer?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default)
        {
            // BVN is now represented by IdentityNumber + IdentityType (BVN) for multi-country support
            return await _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(
                    c => c.IdentityNumber == bvn && c.IdentityType == IdentityType.BVN,
                    cancellationToken);
        }
        public async Task<Customer?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            var referenceObj = CustomerReference.Create(reference);
            return await _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerReference == referenceObj, cancellationToken);
        }
        public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .ToListAsync(cancellationToken);
        }
        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }
        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            _context.Customers.Update(customer);
            return Task.CompletedTask;
        }
        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Customers.AnyAsync(c => c.ContactInfo.Email == email, cancellationToken);
        }
        public async Task<bool> ExistsByBvnAsync(string bvn, CancellationToken cancellationToken = default)
        {
            // BVN is now represented by IdentityNumber + IdentityType (BVN) for multi-country support
            return await _context.Customers.AnyAsync(
                c => c.IdentityNumber == bvn && c.IdentityType == IdentityType.BVN,
                cancellationToken);
        }
        public async Task<Customer?> GetByUserTagAsync(string userTag, CancellationToken cancellationToken = default)
        {
            var normalizedTag = userTag.TrimStart('@', '$').ToUpperInvariant();
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.UserTag != null && c.UserTag.NormalizedTag
                 == normalizedTag, cancellationToken);
        }
        public async Task<bool> IsUserTagAvailableAsync(string userTag, CancellationToken cancellationToken = default)
        {
            var normalizedTag = userTag.TrimStart('@', '$').ToUpperInvariant();
            return !await _context.Customers
                .AnyAsync(c => c.UserTag != null && c.UserTag.NormalizedTag == normalizedTag, cancellationToken);
        }
    }







    //public class CustomerRepository : ICustomerRepository
    //{
    //    private readonly AfriPayDbContext _context;

    //    public CustomerRepository(AfriPayDbContext context)
    //    {
    //        _context = context ?? throw new ArgumentNullException(nameof(context));
    //    }

    //    public async Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Customers
    //            .Include(c => c.Accounts)
    //            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    //    }

    //    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Customers
    //            .Include(c => c.Accounts)
    //            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    //    }

    //    public async Task<Customer?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default)
    //    {
    //        // Use EF.Property to access the converted value directly
    //        return await _context.Customers
    //            .Include(c => c.Accounts)
    //            .FirstOrDefaultAsync(c => EF.Property<string>(c, "BVN") == bvn, cancellationToken);
    //    }

    //    public async Task<Customer?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    //    {
    //        // Use EF.Property to access the converted value directly
    //        return await _context.Customers
    //            .Include(c => c.Accounts)
    //            .FirstOrDefaultAsync(c => EF.Property<string>(c, "CustomerReference") == reference, cancellationToken);
    //    }

    //    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Customers
    //            .Include(c => c.Accounts)
    //            .ToListAsync(cancellationToken);
    //    }

    //    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    //    {
    //        await _context.Customers.AddAsync(customer, cancellationToken);
    //    }

    //    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    //    {
    //        _context.Customers.Update(customer);
    //        return Task.CompletedTask;
    //    }

    //    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.Customers.AnyAsync(c => c.Email == email, cancellationToken);
    //    }

    //    public async Task<bool> ExistsByBvnAsync(string bvn, CancellationToken cancellationToken = default)
    //    {
    //        // Use EF.Property to access the converted value directly
    //        return await _context.Customers.AnyAsync(c => EF.Property<string>(c, "BVN") == bvn, cancellationToken);
    //    }
    //}

}
