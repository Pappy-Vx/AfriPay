using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Configurations;
using Microsoft.EntityFrameworkCore;


namespace AfriPay.DAL.Data
{
    public class AfriPayDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher? _eventDispatcher;

        public AfriPayDbContext(
            DbContextOptions<AfriPayDbContext> options,
            IDomainEventDispatcher? eventDispatcher = null) : base(options)
        {
            _eventDispatcher = eventDispatcher;
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<OnboardingRequest> OnboardingRequests => Set<OnboardingRequest>();
        public DbSet<IdentityVerification> IdentityVerifications => Set<IdentityVerification>();
        public DbSet<AmlScreening> AmlScreenings => Set<AmlScreening>();
        public DbSet<ManualReviewCase> ManualReviewCases => Set<ManualReviewCase>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Apply all configurations from assembly
            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(AfriPayDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new OnboardingRequestConfiguration());
            modelBuilder.ApplyConfiguration(new IdentityVerificationConfiguration());
            modelBuilder.ApplyConfiguration(new AmlScreeningConfiguration());
            modelBuilder.ApplyConfiguration(new ManualReviewCaseConfiguration());
            modelBuilder.ApplyConfiguration(new TransferConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            // Ignore unrelated value objects and derived IdentityNumber types (base IdentityNumber is not ignored for owned mapping)
            modelBuilder.Ignore<AccountId>();
            modelBuilder.Ignore<CustomerId>();
            modelBuilder.Ignore<TransferId>();
            modelBuilder.Ignore<AccountNumber>();
            modelBuilder.Ignore<CustomerReference>();
            //modelBuilder.Ignore<Money>();
            modelBuilder.Ignore<PersonalInfo>();
            //modelBuilder.Ignore<ContactInfo>();
            //modelBuilder.Ignore<Address>();
            modelBuilder.Ignore<PrimaryAccountInfo>();

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events before saving
            // Support both Guid and TransferId aggregate roots
            var guidEntitiesWithEvents = ChangeTracker.Entries<AggregateRoot<Guid>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var transferIdEntitiesWithEvents = ChangeTracker.Entries<AggregateRoot<TransferId>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var events = new List<IDomainEvent>();
            events.AddRange(guidEntitiesWithEvents.SelectMany(e => e.DomainEvents));
            events.AddRange(transferIdEntitiesWithEvents.SelectMany(e => e.DomainEvents));

            guidEntitiesWithEvents.ForEach(e => e.ClearDomainEvents());
            transferIdEntitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            var result = await base.SaveChangesAsync(cancellationToken);
            // Dispatch events after successful save
            if (_eventDispatcher != null)
            {
                foreach (var domainEvent in events)
                {
                    await _eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
                }
            }
            return result;
        }
    }

}
