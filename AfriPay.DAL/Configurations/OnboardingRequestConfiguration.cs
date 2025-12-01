using AfriPay.CORE.Entities;
using AfriPay.CORE.Enums;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AfriPay.DAL.Configurations
{

    public class OnboardingRequestConfiguration : IEntityTypeConfiguration<OnboardingRequest>
    {
        public void Configure(EntityTypeBuilder<OnboardingRequest> builder)
        {
            builder.ToTable("OnboardingRequests");

            // Primary key (assuming OnboardingId is the key; adjust if AggregateRoot.Id is used instead)
            builder.HasKey(x => x.OnboardingId);

            // Properties
            builder.Property(x => x.RequestReference)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(x => x.Country)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(x => x.SelfieUrl)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(x => x.CreatedAt)
                .IsRequired();
            builder.Property(x => x.CompletedAt);
            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);

            // Relationships (if Customer and VirtualAccount are navigations)
            builder.HasOne(x => x.Customer)
                .WithMany() // Adjust if there's a collection on Customer
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.NoAction); // Or Cascade/Restrict as needed
            builder.HasOne(x => x.VirtualAccount)
                .WithMany() // Adjust if there's a collection on Account
                .HasForeignKey(x => x.VirtualAccountId)
                .OnDelete(DeleteBehavior.NoAction); // Or Cascade/Restrict as needed

            // Owned value objects
            builder.OwnsOne(x => x.PersonalInfo, pi =>
            {
                pi.Property(p => p.FirstName).HasMaxLength(100).IsRequired().HasColumnName("FirstName");
                pi.Property(p => p.LastName).HasMaxLength(100).IsRequired().HasColumnName("LastName");
                pi.Property(p => p.MiddleName).HasMaxLength(100).HasColumnName("MiddleName");
                pi.Property(p => p.DateOfBirth).IsRequired().HasColumnName("DateOfBirth");
            });

            builder.OwnsOne(x => x.ContactInfo, ci =>
            {
                ci.Property(c => c.Email).HasMaxLength(100).IsRequired().HasColumnName("Email");
                ci.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired().HasColumnName("PhoneNumber");
            });

            builder.OwnsOne(x => x.IdentityNumber, id =>
            {
                id.Property(i => i.Value).HasMaxLength(50).IsRequired().HasColumnName("IdentityNumber");
                id.Property(i => i.Country).HasMaxLength(3).IsRequired().HasColumnName("IdentityCountry");
            });

            // Ignore computed/convenience properties to prevent mapping errors
            builder.Ignore(x => x.FirstName);
            builder.Ignore(x => x.LastName);
            builder.Ignore(x => x.MiddleName);
            builder.Ignore(x => x.DateOfBirth);
            builder.Ignore(x => x.Email);
            builder.Ignore(x => x.PhoneNumber);
            builder.Ignore(x => x.BVN);
            builder.Ignore(x => x.RequestedAt);

            // Ignore domain events (from AggregateRoot)
            builder.Ignore(x => x.DomainEvents);
            builder.Ignore(x => x.Id);      

            // Indexes
            builder.HasIndex(x => x.RequestReference).IsUnique();
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.VirtualAccountId);
        }
    }


    //public class OnboardingRequestConfiguration : IEntityTypeConfiguration<OnboardingRequest>
    //{
    //    public void Configure(EntityTypeBuilder<OnboardingRequest> builder)
    //    {
    //        builder.ToTable("OnboardingRequests");

    //        builder.HasKey(o => o.OnboardingId);

    //        builder.Property(o => o.OnboardingId)
    //            .ValueGeneratedNever()
    //            .IsRequired();

    //        builder.Property(o => o.RequestReference)
    //            .HasMaxLength(50)
    //            .IsRequired();

    //        builder.HasIndex(o => o.RequestReference).IsUnique();

    //        builder.OwnsOne(x => x.PersonalInfo, pi =>
    //        {
    //            pi.Property(p => p.FirstName).HasMaxLength(100).IsRequired().HasColumnName("FirstName");
    //            pi.Property(p => p.LastName).HasMaxLength(100).IsRequired().HasColumnName("LastName");
    //            pi.Property(p => p.MiddleName).HasMaxLength(100).HasColumnName("MiddleName");
    //            pi.Property(p => p.DateOfBirth).IsRequired().HasColumnName("DateOfBirth");
    //            pi.Property(p => p.Gender).HasMaxLength(10).IsRequired().HasColumnName("Gender");
    //        });

    //        builder.OwnsOne(x => x.ContactInfo, ci =>
    //        {
    //            ci.Property(c => c.Email).HasMaxLength(255).IsRequired().HasColumnName("Email");
    //            ci.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired().HasColumnName("PhoneNumber");
    //            ci.Property(c => c.AlternativePhoneNumber).HasMaxLength(20).HasColumnName("AlternativePhoneNumber");
    //        });

    //        builder.HasIndex(o => o.Email);
    //        builder.HasIndex(o => o.PhoneNumber);

    //        builder.OwnsOne(x => x.IdentityNumber, id =>
    //        {
    //            id.Property(i => i.Value).HasMaxLength(50).IsRequired().HasColumnName("IdentityNumber");
    //            id.Property(i => i.Country).HasMaxLength(10).IsRequired().HasColumnName("IdentityCountry");
    //        });

    //        builder.Property(x => x.Country).IsRequired().HasConversion<int>();
    //        builder.Property(x => x.SelfieUrl).HasMaxLength(500).IsRequired();
    //        builder.Property(o => o.Status).IsRequired().HasConversion<int>();

    //        builder.Property(o => o.CustomerId)
    //            .HasConversion(
    //                id => id != null ? id.Value : (Guid?)null,
    //                value => value.HasValue ? CustomerId.Create(value.Value) : null)
    //            .HasColumnName("CustomerId");

    //        builder.Property(o => o.VirtualAccountId)
    //            .HasConversion(
    //                id => id != null ? id.Value : (Guid?)null,
    //                value => value.HasValue ? AccountId.Create(value.Value) : null)
    //            .HasColumnName("VirtualAccountId");

    //        builder.Property(o => o.CreatedAt).IsRequired();
    //        builder.Property(o => o.CompletedAt);
    //        builder.Property(o => o.FailureReason).HasMaxLength(500);

    //        builder.HasIndex(o => o.Status);
    //        builder.HasIndex(o => o.CustomerId);
    //        builder.HasIndex(o => o.CreatedAt);

    //        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    //        builder.HasOne(x => x.VirtualAccount).WithMany().HasForeignKey(x => x.VirtualAccountId).OnDelete(DeleteBehavior.Restrict);

    //        builder.Ignore(o => o.DomainEvents);
    //        builder.Ignore(o => o.Id);
    //        builder.Ignore(o => o.BVN);
    //        builder.Ignore(o => o.RequestedAt);
    //    }
    //}
}
