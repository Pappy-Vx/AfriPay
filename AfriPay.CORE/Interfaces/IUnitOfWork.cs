namespace AfriPay.CORE.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IAccountRepository Accounts { get; }
    IOnboardingRequestRepository OnboardingRequests { get; }
    ITransferRepository Transfers { get; }
    ITransactionRepository Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}