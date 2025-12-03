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

            // =====================================================================
            // AUTHENTICATION PROPERTIES
            // =====================================================================

            // Password hash for authentication
            builder.Property(c => c.PasswordHash)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired();

            // Account active status
            builder.Property(c => c.IsActive)
                .HasColumnName("IsActive")
                .IsRequired()
                .HasDefaultValue(true);

            // =====================================================================
            // CONTACT INFORMATION (OWNED ENTITY)
            // =====================================================================

            builder.OwnsOne(c => c.ContactInfo, ci =>
            {
                ci.Property(x => x.Email)
                    .HasColumnName("Email")
                    .HasMaxLength(200)
                    .IsRequired();

                ci.Property(x => x.PhoneNumber)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired();
            });

            // =====================================================================
            // ADDRESS (OWNED ENTITY)
            // =====================================================================

            builder.OwnsOne(c => c.Address, addr =>
            {
                addr.Property(a => a.Street)
                    .HasColumnName("AddressStreet")
                    .HasMaxLength(200)
                    .IsRequired();

                addr.Property(a => a.City)
                    .HasColumnName("AddressCity")
                    .HasMaxLength(100)
                    .IsRequired();

                addr.Property(a => a.State)
                    .HasColumnName("AddressState")
                    .HasMaxLength(100)
                    .IsRequired();

                addr.Property(a => a.Country)
                    .HasColumnName("AddressCountry")
                    .HasMaxLength(3)
                    .IsRequired();

                addr.Property(a => a.PostalCode)
                    .HasColumnName("AddressPostalCode")
                    .HasMaxLength(20);

                // Ignore the computed FullAddress property
                addr.Ignore(a => a.FullAddress);
            });

            builder.OwnsOne(c => c.UserTag, tag =>
            {
                tag.Property(t => t.Value)
                    .HasColumnName("UserTag")
                    .HasMaxLength(20);

                tag.Property(t => t.NormalizedTag)
                    .HasColumnName("NormalizedUserTag")
                    .HasMaxLength(20);

                tag.HasIndex(t => t.NormalizedTag)
                    .IsUnique()
                    .HasFilter("[NormalizedUserTag] IS NOT NULL");
            });

            builder.Property(c => c.UserTagSetAt)
                .HasColumnName("UserTagSetAt");



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
                .HasColumnName("IsBvnVerified")
                .IsRequired()
                .HasDefaultValue(false);

            // =====================================================================
            // AUDIT FIELDS
            // =====================================================================

            builder.Property(c => c.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            // =====================================================================
            // INDEXES
            // =====================================================================

            // Unique index on Email for fast lookup and uniqueness
            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Customers_Email");

            // Unique index on BVN
            builder.HasIndex("BVN")
                .IsUnique()
                .HasDatabaseName("IX_Customers_BVN");

            // Unique index on CustomerReference
            builder.HasIndex("CustomerReference")
                .IsUnique()
                .HasDatabaseName("IX_Customers_CustomerReference");

            // Index on IsActive for filtering active customers
            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Customers_IsActive");

            // Composite index for authentication queries (Email + IsActive)
            builder.HasIndex(c => new { c.Email, c.IsActive })
                .HasDatabaseName("IX_Customers_Email_IsActive");

            // =====================================================================
            // RELATIONSHIPS
            // =====================================================================

            // One-to-many: Customer -> Accounts
            builder.HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many: Customer -> OnboardingRequests
            builder.HasMany(c => c.OnboardingRequests)
                .WithOne(o => o.Customer)
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================================
            // IGNORED PROPERTIES
            // =====================================================================

            // Ignore domain events (not persisted)
            builder.Ignore(c => c.DomainEvents);

            // Ignore base class Id property (using CustomerId instead)
            builder.Ignore(c => c.Id);

            // Ignore computed Email property (mapped from ContactInfo.Email)
            builder.Ignore(c => c.Email);
        }
    }
}