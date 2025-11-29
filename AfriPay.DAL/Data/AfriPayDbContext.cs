using AfriPay.CORE.Entities;
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
    public class AfriPayDbContext : DbContext
    {
        public AfriPayDbContext(DbContextOptions<AfriPayDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<OnboardingRequest> OnboardingRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CustomerConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new OnboardingRequestConfiguration());

            modelBuilder.Ignore<AccountId>();
            modelBuilder.Ignore<CustomerId>();
            modelBuilder.Ignore<AccountNumber>();
            modelBuilder.Ignore<BVN>();
            modelBuilder.Ignore<CustomerReference>();
            modelBuilder.Ignore<Money>();
        }
    }

}
