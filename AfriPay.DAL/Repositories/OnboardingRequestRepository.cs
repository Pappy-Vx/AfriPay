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
                .FirstOrDefaultAsync(o => o.OnboardingId == onboardingId, cancellationToken);
        }
        public async Task<OnboardingRequest?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .Include(o => o.Customer)
                .Include(o => o.VirtualAccount)
                .FirstOrDefaultAsync(o => o.RequestReference == reference, cancellationToken);
        }
        public async Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default)
        {
            var bvnObj = BVN.Create(bvn);
            return await _context.OnboardingRequests
                .FirstOrDefaultAsync(o => o.BVN == bvnObj, cancellationToken);
        }
        public async Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .Where(o => o.Status != OnboardingStatus.Completed && o.Status != OnboardingStatus.Failed)
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

        public async Task<bool> ExistsAsync(string email, string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _context.OnboardingRequests
                .AnyAsync(o => o.Email == email || o.PhoneNumber == phoneNumber, cancellationToken);
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
    //        // Use EF.Property to access the converted value directly
    //        return await _context.OnboardingRequests
    //            .FirstOrDefaultAsync(o => EF.Property<string>(o, "BVN") == bvn, cancellationToken);
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
    //}




}
