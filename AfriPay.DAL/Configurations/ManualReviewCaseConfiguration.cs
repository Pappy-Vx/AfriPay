using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AfriPay.DAL.Configurations
{
    public class ManualReviewCaseConfiguration : IEntityTypeConfiguration<ManualReviewCase>
    {
        public void Configure(EntityTypeBuilder<ManualReviewCase> builder)
        {
            builder.ToTable("ManualReviewCases");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OnboardingId).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasConversion<int>();
            builder.Property(x => x.Priority).IsRequired().HasConversion<int>();
            builder.Property(x => x.AssignedTo).HasMaxLength(100);
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Resolution).HasMaxLength(100);
            builder.Property(x => x.ResolutionNotes).HasMaxLength(1000);

            builder.Property(x => x.Comments)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

            builder.HasIndex(x => x.OnboardingId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => x.AssignedTo);

            builder.Ignore(x => x.DomainEvents);
        }
    }
}
