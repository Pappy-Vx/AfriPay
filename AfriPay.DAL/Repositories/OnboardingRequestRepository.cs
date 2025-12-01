using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
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

    public class OnboardingRequestRepository : IOnboardingRequestRepository
    {
        private readonly AfriPayDbContext _context;

        public OnboardingRequestRepository(AfriPayDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<OnboardingRequest?> GetByIdAsync(Guid onboardingId, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .Include(o => o.Customer)
                .Include(o => o.VirtualAccount)
                //.Include(o => o.)
                //.Include(o => o.AmlScreening)
                //.Include(o => o.ManualReviewCases)
                .FirstOrDefaultAsync(o => o.Id == onboardingId, cancellationToken);
        }

        public async Task<OnboardingRequest?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .Include(o => o.Customer)
                .Include(o => o.VirtualAccount)
                //.Include(o => o.IdentityVerification)
                //.Include(o => o.AmlScreening)
                //.Include(o => o.ManualReviewCases)
                .FirstOrDefaultAsync(o => o.RequestReference == reference, cancellationToken);
        }

        public async Task<OnboardingRequest?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened column from IdentityNumber value object
            return await _context.OnboardingRequests
                .FirstOrDefaultAsync(o => EF.Property<string>(o, "IdentityNumber") == identityNumber, cancellationToken);
        }

        public async Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                //.Include(o => o.IdentityVerification)
                //.Include(o => o.AmlScreening)
                .Where(o => o.Status != OnboardingStatus.Completed && o.Status != OnboardingStatus.Failed)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(OnboardingRequest request, CancellationToken cancellationToken = default)
        {
            await _context.OnboardingRequests.AddAsync(request, cancellationToken);
        }

        public Task UpdateAsync(OnboardingRequest request, CancellationToken cancellationToken = default)
        {
            _context.OnboardingRequests.Update(request);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid onboardingId, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .AnyAsync(o => o.OnboardingId == onboardingId, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened Email column from ContactInfo value object
            return await _context.OnboardingRequests
                .AnyAsync(o => o.ContactInfo.Email == email , cancellationToken);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened PhoneNumber column from ContactInfo value object
            return await _context.OnboardingRequests
                .AnyAsync(o => o.ContactInfo.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task<bool> ExistsByEmailOrPhoneAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened columns
            return await _context.OnboardingRequests
                .AnyAsync(o =>
                     o.ContactInfo.Email == email ||
                o.ContactInfo.PhoneNumber == phoneNumber,
                    cancellationToken);
        }

       

        public async Task<OnboardingRequest?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened Email column
            return await _context.OnboardingRequests
                .FirstOrDefaultAsync( o => o.ContactInfo.Email == email, cancellationToken);
        }

        public async Task<OnboardingRequest?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            // Use EF.Property to access the flattened PhoneNumber column
            
            return await _context.OnboardingRequests
                .FirstOrDefaultAsync(o => o.ContactInfo.PhoneNumber == phoneNumber, cancellationToken);
        }

        public async Task<IEnumerable<OnboardingRequest>> GetByStatusAsync(OnboardingStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .Where(o => o.Status == status)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OnboardingRequest>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                //.Where(o => o.Country == country)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default)
         {
             var bvnObj = BVN.Create(bvn);
             return await _context.OnboardingRequests
                 .FirstOrDefaultAsync(o => o.BVN == bvnObj, cancellationToken);
        }

    }


    //public class OnboardingRequestRepository : IOnboardingRequestRepository
    //{
    //    private readonly AfriPayDbContext _context;
    //    public OnboardingRequestRepository(AfriPayDbContext context)
    //    {
    //        _context = context ?? throw new ArgumentNullException(nameof(context));
    //    }
    //    public async Task<OnboardingRequest?> GetByIdAsync(Guid onboardingId, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.OnboardingRequests
    //            .Include(o => o.Customer)
    //            .Include(o => o.VirtualAccount)
    //            .FirstOrDefaultAsync(o => o.OnboardingId == onboardingId, cancellationToken);
    //    }
    //    public async Task<OnboardingRequest?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.OnboardingRequests
    //            .Include(o => o.Customer)
    //            .Include(o => o.VirtualAccount)
    //            .FirstOrDefaultAsync(o => o.RequestReference == reference, cancellationToken);
    //    }
    //    public async Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default)
    //    {
    //        var bvnObj = BVN.Create(bvn);
    //        return await _context.OnboardingRequests
    //            .FirstOrDefaultAsync(o => o.BVN == bvnObj, cancellationToken);
    //    }
    //    public async Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    //    {
    //        return await _context.OnboardingRequests
    //            .Where(o => o.Status != OnboardingStatus.Completed && o.Status != OnboardingStatus.Failed)
    //            .ToListAsync(cancellationToken);
    //    }
    //    public async Task AddAsync(OnboardingRequest request, CancellationToken cancellationToken = default)
    //    {
    //        await _context.OnboardingRequests.AddAsync(request, cancellationToken);
    //    }
    //    public Task UpdateAsync(OnboardingRequest request, CancellationToken cancellationToken = default)
    //    {
    //        _context.OnboardingRequests.Update(request);
    //        return Task.CompletedTask;
    //    }
    //    public async Task<bool> ExistsAsync(Guid onboardingId, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.OnboardingRequests
    //            .AnyAsync(o => o.OnboardingId == onboardingId, cancellationToken);
    //    }

    //    public async Task<bool> ExistsAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
    //    {
    //        return await _context.OnboardingRequests
    //            .AnyAsync(o => o.Email == email || o.PhoneNumber == phoneNumber, cancellationToken);
    //    }
    //}













}
