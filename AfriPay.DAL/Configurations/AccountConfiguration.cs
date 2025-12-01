using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.DAL.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.AccountId)
                .HasConversion(
                    id => id.Value,
                    value => AccountId.Create(value))
                .IsRequired();



            // Configure AccountNumber as owned type (Value Object)
            builder.Property(a => a.AccountNumber)
                .HasConversion(
                    accountNumber => accountNumber.Value,
                    value => AccountNumber.Create(value))
                .HasColumnName("AccountNumber")
                .HasMaxLength(10)
                .IsRequired();

            //builder.Property(a => a.AccountType)
            //    .HasConversion<string>()
            //    .HasMaxLength(20)
            //    .IsRequired();

            builder.OwnsOne(a => a.Balance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("BalanceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("BalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Configure ReservedBalance as owned entity (THIS WAS MISSING!)
            builder.OwnsOne(a => a.ReservedBalance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("ReservedBalanceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("ReservedBalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(a => a.CustomerId)
                .HasConversion(
                    id => id.Value,
                    value => CustomerId.Create(value))
                .IsRequired();

            builder.Property(a => a.DateOpened)
                .IsRequired();

            builder.Property(a => a.IsActive)
                .IsRequired();

            builder.Property(a => a.ProviderReference)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(a => a.AccountNumber).IsUnique();
            builder.HasIndex(a => a.CustomerId);
            builder.HasIndex(a => a.ProviderReference);

            builder.Property(a => a.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

            // Ignore domain events
            builder.Ignore(a => a.DomainEvents);
        }
    }
}
