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

            // Primary key configuration
            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerId)
                .HasConversion(
                    id => id.Value,
                    value => CustomerId.Create(value))
                .ValueGeneratedNever()
                .IsRequired();

            //builder.Property(c => c.CustomerReference)
            //    .HasConversion(
            //        cr => cr.Value,
            //        value => CustomerReference.Create(value))
            //    .HasMaxLength(50)
            //    .IsRequired();

            builder.Property(c => c.CustomerReference)
.HasConversion(
cr => cr.Value,
value => CustomerReference.Create(value))
.HasColumnName("CustomerReference")
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

            // Fixed BVN conversion with proper column name
            builder.Property(c => c.BVN)
.HasConversion(
bvn => bvn.Value,
value => BVN.Create(value))
.HasColumnName("BVN")
.HasMaxLength(11)
.IsRequired();

            //builder.Property(c => c.BVN)
            //    .HasColumnName("BVN")
            //    .HasConversion(
            //        bvn => bvn.Value,
            //        value => BVN.Create(value))
            //    .HasMaxLength(11)
            //    .IsRequired();


            builder.Property(c => c.IsBvnVerified)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.Email).IsUnique();
            builder.HasIndex("BVN").IsUnique();
            builder.HasIndex("CustomerReference").IsUnique();

            // Relationships - use shadow properties for foreign keys
            builder.HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.OnboardingRequests)
                .WithOne(o => o.Customer)
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);

            // Ignore domain events and Id property from base class
            builder.Ignore(c => c.DomainEvents);
            builder.Ignore(c => c.Id);
        }
    }

}
