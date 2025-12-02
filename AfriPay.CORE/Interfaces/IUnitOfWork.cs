namespace AfriPay.CORE.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IAccountRepository Accounts { get; }
    IOnboardingRequestRepository OnboardingRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}