using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfriPay.DAL.Configurations
{

    public class AmlScreeningConfiguration : IEntityTypeConfiguration<AmlScreening>
    {
        public void Configure(EntityTypeBuilder<AmlScreening> builder)
        {
            builder.ToTable("AmlScreenings");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.OnboardingId)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RiskLevel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RiskScore)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(x => x.ScreeningProvider)
                .HasMaxLength(100);

            builder.Property(x => x.ScreeningReference)
                .HasMaxLength(100);

            builder.Property(x => x.ScreeningDate);

            // Flags - stored as JSON or comma-separated
            builder.Property(x => x.Flags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(1000);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.RawResponse)
                .HasMaxLength(4000);

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
            builder.HasIndex(x => x.RiskLevel);
            builder.HasIndex(x => x.ScreeningReference);

            // Ignore domain events
            builder.Ignore(x => x.DomainEvents);
        }
    }



    //public class AmlScreeningConfiguration : IEntityTypeConfiguration<AmlScreening>
    //{
    //    public void Configure(EntityTypeBuilder<AmlScreening> builder)
    //    {
    //        builder.ToTable("AmlScreenings");

    //        builder.HasKey(x => x.Id);

    //        builder.Property(x => x.OnboardingId).IsRequired();
    //        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
    //        builder.Property(x => x.RiskScore).HasPrecision(5, 2);
    //        builder.Property(x => x.ProviderReference).HasMaxLength(100);
    //        builder.Property(x => x.ProviderResponse).HasMaxLength(2000);
    //        builder.Property(x => x.ScreenedAt).IsRequired();
    //        builder.Property(x => x.RequiresManualReview).IsRequired();
    //        builder.Property(x => x.ReviewReason).HasMaxLength(500);

    //        builder.HasIndex(x => x.OnboardingId);
    //        builder.HasIndex(x => x.Status);
    //        builder.HasIndex(x => x.RequiresManualReview);

    //        builder.Ignore(x => x.DomainEvents);
    //    }
    //}
}
