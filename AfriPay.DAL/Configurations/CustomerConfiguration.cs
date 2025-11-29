using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.DAL.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerId)
                .HasConversion(
                    id => id.Value,
                    value => CustomerId.Create(value))
                .IsRequired();

            builder.Property(c => c.CustomerReference)
                .HasConversion(
                    cr => cr.Value,
                    value => CustomerReference.Create(value))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(c => c.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.BVN)
                .HasConversion(
                    bvn => bvn.Value,
                    value => BVN.Create(value))
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(c => c.IsBvnVerified)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.Email).IsUnique();
            builder.HasIndex(c => c.BVN).IsUnique();
            builder.HasIndex(c => c.CustomerReference).IsUnique();

            // Relationships
            builder.HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.OnboardingRequests)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ignore domain events
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
