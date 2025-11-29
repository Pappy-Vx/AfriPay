using AfriPay.CORE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.Interfaces
{
    public interface IOnboardingRequestRepository
    {
        Task<OnboardingRequest?> GetByIdAsync(Guid onboardingId, CancellationToken cancellationToken = default);
        Task<OnboardingRequest?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
        Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default);
        Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
        Task AddAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid onboardingId, CancellationToken cancellationToken = default);
    }
}
