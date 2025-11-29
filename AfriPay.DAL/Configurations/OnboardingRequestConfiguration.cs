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
    public class OnboardingRequestConfiguration : IEntityTypeConfiguration<OnboardingRequest>
    {
        public void Configure(EntityTypeBuilder<OnboardingRequest> builder)
        {
            builder.ToTable("OnboardingRequests");

            builder.HasKey(o => o.OnboardingId);

            builder.Property(o => o.RequestReference)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(o => o.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(o => o.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.BVN)
                .HasConversion(
                    bvn => bvn.Value,
                    value => BVN.Create(value))
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.CustomerId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? CustomerId.Create(value.Value) : null);

            builder.Property(o => o.VirtualAccountId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? AccountId.Create(value.Value) : null);

            builder.Property(o => o.RequestedAt)
                .IsRequired();

            builder.Property(o => o.FailureReason)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(o => o.RequestReference).IsUnique();
            builder.HasIndex(o => o.BVN);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.CustomerId);

            // Ignore domain events
            builder.Ignore(o => o.DomainEvents);
        }
    }

}
