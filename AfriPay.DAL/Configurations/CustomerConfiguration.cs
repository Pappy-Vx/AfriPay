using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfriPay.DAL.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            // =====================================================================
            // PRIMARY KEY CONFIGURATION
            // =====================================================================
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

            // =====================================================================
            // BASIC INFORMATION
            // =====================================================================
            builder.Property(c => c.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();

            // Flat properties for backward compatibility
            //builder.Property(c => c.Email)
            //    .HasColumnName("Email")
            //    .HasMaxLength(200)
            //    .IsRequired();

            //builder.Property(c => c.PhoneNumber)
            //    .HasColumnName("PhoneNumber")
            //    .HasMaxLength(20)
            //    .IsRequired();

            // =====================================================================
            // IDENTITY VERIFICATION (Multi-country support)
            // =====================================================================
            builder.Property(c => c.IdentityNumber)
                .HasColumnName("IdentityNumber")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.IdentityType)
                .HasColumnName("IdentityType")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.IsIdentityVerified)
                .HasColumnName("IsIdentityVerified")
                .IsRequired()
                .HasDefaultValue(false);

            // Obsolete BVN property (kept for backward compatibility)
            builder.Property(c => c.IsBvnVerified)
                .HasColumnName("IsBvnVerified")
                .IsRequired()
                .HasDefaultValue(false);

            // =====================================================================
            // AUTHENTICATION PROPERTIES
            // =====================================================================
            builder.Property(c => c.PasswordHash)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .HasColumnName("IsActive")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.Status)
                .HasColumnName("Status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // =====================================================================
            // USER TAG (OWNED ENTITY)
            // =====================================================================
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

            // =====================================================================
            // PERSONAL INFO (OWNED ENTITY - OPTIONAL)
            // =====================================================================
            //builder.OwnsOne(c => c.PersonalInfo, pi =>
            //{
            //    pi.Property(p => p.DateOfBirth)
            //        .HasColumnName("DateOfBirth");

            //    pi.Property(p => p.Gender)
            //        .HasColumnName("Gender")
            //        .HasMaxLength(20);

            //    pi.Property(p => p.Nationality)
            //        .HasColumnName("Nationality")
            //        .HasMaxLength(100);
            //});

            // =====================================================================
            // CONTACT INFORMATION (OWNED ENTITY - OPTIONAL)
            // Note: Maps to same columns as flat Email/PhoneNumber for sync
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
            // ADDRESS (OWNED ENTITY - OPTIONAL)
            // =====================================================================
            builder.OwnsOne(c => c.Address, addr =>
            {
                addr.Property(a => a.Street)
                    .HasColumnName("AddressStreet")
                    .HasMaxLength(200);

                addr.Property(a => a.City)
                    .HasColumnName("AddressCity")
                    .HasMaxLength(100);

                addr.Property(a => a.State)
                    .HasColumnName("AddressState")
                    .HasMaxLength(100);

                addr.Property(a => a.Country)
                    .HasColumnName("AddressCountry")
                    .HasMaxLength(3);

                addr.Property(a => a.PostalCode)
                    .HasColumnName("AddressPostalCode")
                    .HasMaxLength(20);

                // Ignore the computed FullAddress property
                addr.Ignore(a => a.FullAddress);
            });

            // =====================================================================
            // AUDIT FIELDS
            // =====================================================================
            builder.Property(c => c.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .HasColumnName("UpdatedAt");

            // =====================================================================
            // INDEXES
            // =====================================================================

            // Unique index on Email for fast lookup and uniqueness
            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Customers_Email");

            // Unique index on CustomerReference
            builder.HasIndex(c => c.CustomerReference)
                .HasDatabaseName("IX_Customers_CustomerReference");

            // Composite index on IdentityNumber + IdentityType for multi-country support
            builder.HasIndex(c => new { c.IdentityNumber, c.IdentityType })
                .IsUnique()
                .HasDatabaseName("IX_Customers_Identity");

            // Index on IsActive for filtering active customers
            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Customers_IsActive");

            // Composite index for authentication queries (Email + IsActive)
            builder.HasIndex(c => new { c.Email, c.IsActive })
                .HasDatabaseName("IX_Customers_Email_IsActive");

            // Index on Status for filtering by customer status
            builder.HasIndex(c => c.Status)
                .HasDatabaseName("IX_Customers_Status");

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
        }
    }
}