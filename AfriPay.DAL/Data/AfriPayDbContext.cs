using AfriPay.CORE.Common;
using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using AfriPay.DAL.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Ignore unrelated value objects and derived IdentityNumber types (base IdentityNumber is not ignored for owned mapping)
            modelBuilder.Ignore<AccountId>();
            modelBuilder.Ignore<CustomerId>();
            modelBuilder.Ignore<AccountNumber>();
            modelBuilder.Ignore<CustomerReference>();
            modelBuilder.Ignore<Money>();
            modelBuilder.Ignore<PersonalInfo>();
            modelBuilder.Ignore<ContactInfo>();
            modelBuilder.Ignore<Address>();
            modelBuilder.Ignore<PrimaryAccountInfo>();
            modelBuilder.Ignore<BVN>();
            modelBuilder.Ignore<GhanaCard>();
            modelBuilder.Ignore<GhanaCardNumber>(); // Added if this variant exists; remove if merged
            modelBuilder.Ignore<KenyaNationalID>();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events before saving
            var entitiesWithEvents = ChangeTracker.Entries<AggregateRoot<Guid>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();
            var events = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());
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
