using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.DAL.Data;

namespace AfriPay.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AfriPayDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;

    private ICustomerRepository? _customers;
    private IAccountRepository? _accounts;
    private IOnboardingRequestRepository? _onboardingRequests;

    public UnitOfWork(AfriPayDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
    public IOnboardingRequestRepository OnboardingRequests => _onboardingRequests ??= new OnboardingRequestRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all aggregate roots (regardless of TId type)
        var allEntries = _context.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().BaseType?.IsGenericType == true &&
                       e.Entity.GetType().BaseType.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            .Select(e => e.Entity)
            .ToList();

        // Collect domain events using reflection to access DomainEvents property
        var events = new List<IDomainEvent>();
        foreach (var entity in allEntries)
        {
            var domainEventsProperty = entity.GetType().BaseType?.GetProperty("DomainEvents");
            if (domainEventsProperty != null)
            {
                var domainEvents = domainEventsProperty.GetValue(entity) as IReadOnlyCollection<IDomainEvent>;
                if (domainEvents != null && domainEvents.Any())
                {
                    events.AddRange(domainEvents);

                    // Clear domain events
                    var clearMethod = entity.GetType().BaseType?.GetMethod("ClearDomainEvents");
                    clearMethod?.Invoke(entity, null);
                }
            }
        }

        // Save to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        foreach (var domainEvent in events)
        {
            await _eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return result;
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var result = await SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}