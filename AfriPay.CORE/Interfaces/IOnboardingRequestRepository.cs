using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
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
        Task<OnboardingRequest?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default);
        Task<OnboardingRequest?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<OnboardingRequest?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<OnboardingRequest>> GetByStatusAsync(OnboardingStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<OnboardingRequest>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
        Task AddAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid onboardingId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailOrPhoneAsync(string email, string phoneNumber, CancellationToken cancellationToken = default);
        //Task<bool> ExistsByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default);
        Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default);
        //Task<bool> ExistsAsync(string email, string phoneNumber, CancellationToken cancellationToken = default);


    }


    //public interface IOnboardingRequestRepository
    //{
    //    Task<OnboardingRequest?> GetByIdAsync(Guid onboardingId, CancellationToken cancellationToken = default);
    //    Task<OnboardingRequest?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    //    Task<OnboardingRequest?> GetByBvnAsync(string bvn, CancellationToken cancellationToken = default);
    //    Task<IEnumerable<OnboardingRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    //    Task AddAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
    //    Task UpdateAsync(OnboardingRequest request, CancellationToken cancellationToken = default);
    //    Task<bool> ExistsAsync(Guid onboardingId, CancellationToken cancellationToken = default);
    //    Task<bool> ExistsAsync(string email, string phoneNumber, CancellationToken cancellationToken = default);
    //}
}
