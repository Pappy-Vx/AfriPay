using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfriPay.DAL.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionReference)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.TransactionReference)
            .IsUnique();

        // Value Converters for IDs
        builder.Property(t => t.AccountId)
            .HasConversion(
                id => id.Value,
                value => AccountId.Create(value));

        builder.Property(t => t.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value));

        // Configure Money as Owned Type - MUST map both Amount and Currency
        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

            money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
        });

        builder.Property(t => t.BalanceBefore)
            .HasPrecision(18, 2);

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 2);

        builder.Property(t => t.Direction)
            .HasConversion<int>();

        builder.Property(t => t.Narration)
            .HasMaxLength(200);

        // TransferId - nullable, use value converter
        builder.Property(t => t.TransferId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? TransferId.Create(value.Value) : null);

        // Indexes
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.CreatedAt);
    }
}