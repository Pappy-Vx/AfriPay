using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfriPay.DAL.Configurations
{
    public class IdentityVerificationConfiguration : IEntityTypeConfiguration<IdentityVerification>
    {
        public void Configure(EntityTypeBuilder<IdentityVerification> builder)
        {
            builder.ToTable("IdentityVerifications");
            // Primary key
            builder.HasKey(x => x.Id);
            // Properties
            builder.Property(x => x.OnboardingId)
                .IsRequired();
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(x => x.VerificationProvider)
                .HasMaxLength(100);
            builder.Property(x => x.VerificationReference)
                .HasMaxLength(100);
            builder.Property(x => x.VerificationDate);
            builder.Property(x => x.MatchScore)
                .HasPrecision(5, 2);
            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);
            builder.Property(x => x.RawResponse)
                .HasMaxLength(4000);
            // Value Object - IdentityNumber (mapped as owned type; polymorphism handled in domain code)
            builder.OwnsOne(x => x.IdentityNumber, identityNumber =>
            {
                // Common properties for the base type
                identityNumber.Property(i => i.Value)
                    .HasColumnName("IdentityNumber")
                    .HasMaxLength(50)
                    .IsRequired();
                identityNumber.Property(i => i.Country)
                    .HasColumnName("IdentityCountry")
                    .HasMaxLength(3)
                    .IsRequired();
            });
            // Auditable properties (inherited from AuditableEntity)
            builder.Property(x => x.CreatedAt)
                .IsRequired();
            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.UpdatedAt);
            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100);
            // Indexes
            builder.HasIndex(x => x.OnboardingId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.VerificationReference);
            // Ignore domain events
            builder.Ignore(x => x.DomainEvents);
        }
    }
}



    //public class IdentityVerificationConfiguration : IEntityTypeConfiguration<IdentityVerification>
    //{
    //    public void Configure(EntityTypeBuilder<IdentityVerification> builder)
    //    {
    //        builder.ToTable("IdentityVerifications");

    //        builder.HasKey(x => x.Id);

    //        builder.Property(x => x.OnboardingId).IsRequired();

    //        builder.OwnsOne(x => x.IdentityNumber, id =>
    //        {
    //            id.Property(i => i.Value).HasMaxLength(50).IsRequired().HasColumnName("IdentityNumber");
    //            id.Property(i => i.Country).HasMaxLength(10).IsRequired().HasColumnName("IdentityCountry");
    //        });

    //        builder.Property(x => x.IsVerified).IsRequired();
    //        builder.Property(x => x.VerifiedFirstName).HasMaxLength(100);
    //        builder.Property(x => x.VerifiedLastName).HasMaxLength(100);
    //        builder.Property(x => x.VerifiedMiddleName).HasMaxLength(100);
    //        builder.Property(x => x.VerifiedPhone).HasMaxLength(20);
    //        builder.Property(x => x.NameMatchScore).HasPrecision(5, 2);
    //        builder.Property(x => x.PhotoMatchScore).HasPrecision(5, 2);
    //        builder.Property(x => x.ProviderReference).HasMaxLength(100);
    //        builder.Property(x => x.ProviderResponse).HasMaxLength(2000);
    //        builder.Property(x => x.FailureReason).HasMaxLength(500);
    //        builder.Property(x => x.VerifiedAt).IsRequired();

    //        builder.HasIndex(x => x.OnboardingId);
    //        builder.HasIndex(x => x.IsVerified);

    //        builder.Ignore(x => x.DomainEvents);
    //    }
    //}

