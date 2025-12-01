using AfriPay.APP.DTOs;
using AfriPay.CORE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Services
{
    public interface IOnboardingService
    {
        Task<Result<OnboardingResponse>> StartOnboardingAsync(
            OnboardingStartRequest request,
            CancellationToken cancellationToken = default);

        Task<Result<OnboardingResponse>> GetOnboardingStatusAsync(
            Guid onboardingId,
            CancellationToken cancellationToken = default);
    }
}
