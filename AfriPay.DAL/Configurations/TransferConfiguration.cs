using AfriPay.CORE.Entities;
using AfriPay.CORE.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AfriPay.DAL.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
  public void Configure(EntityTypeBuilder<Transfer> builder)
  {
    builder.ToTable("Transfers");

    // Use Value Converter for Id (TransferId)
    builder.HasKey(t => t.Id);

    builder.Property(t => t.Id)
        .HasConversion(
            id => id.Value,
            value => TransferId.Create(value))
        .HasColumnName("Id");

    builder.Property(t => t.TransferReference)
        .IsRequired()
        .HasMaxLength(50);

    builder.HasIndex(t => t.TransferReference)
        .IsUnique();

    builder.Property(t => t.IdempotencyKey)
        .HasMaxLength(100);

    builder.HasIndex(t => t.IdempotencyKey)
        .IsUnique()
        .HasFilter("[IdempotencyKey] IS NOT NULL");

    // Source - Value Converters
    builder.Property(t => t.SourceAccountId)
        .HasConversion(
            id => id.Value,
            value => AccountId.Create(value));

    builder.Property(t => t.SourceCustomerId)
        .HasConversion(
            id => id.Value,
            value => CustomerId.Create(value));

    // Destination - Value Converters
    builder.Property(t => t.DestinationAccountId)
        .HasConversion(
            id => id.Value,
            value => AccountId.Create(value));

    builder.Property(t => t.DestinationCustomerId)
        .HasConversion(
            id => id.Value,
            value => CustomerId.Create(value));

    builder.Property(t => t.DestinationUserTag)
        .HasMaxLength(50);

    // Money fields - MUST map both Amount and Currency
    builder.OwnsOne(t => t.Amount, money =>
    {
      money.Property(m => m.Amount)
              .HasColumnName("Amount")
              .HasPrecision(18, 2)
              .IsRequired();

      money.Property(m => m.Currency)
              .HasColumnName("AmountCurrency")
              .HasMaxLength(3)
              .IsRequired();
    });

    builder.OwnsOne(t => t.Fee, money =>
    {
      money.Property(m => m.Amount)
              .HasColumnName("Fee")
              .HasPrecision(18, 2);

      money.Property(m => m.Currency)
              .HasColumnName("FeeCurrency")
              .HasMaxLength(3);
    });

    builder.OwnsOne(t => t.TotalDebitAmount, money =>
    {
      money.Property(m => m.Amount)
              .HasColumnName("TotalDebitAmount")
              .HasPrecision(18, 2)
              .IsRequired();

      money.Property(m => m.Currency)
              .HasColumnName("TotalDebitCurrency")
              .HasMaxLength(3)
              .IsRequired();
    });

    // Enums
    builder.Property(t => t.Status)
        .HasConversion<int>();

    builder.Property(t => t.Type)
        .HasConversion<int>();

    // String fields
    builder.Property(t => t.Description)
        .HasMaxLength(500);

    builder.Property(t => t.Narration)
        .HasMaxLength(200);

    builder.Property(t => t.FailureReason)
        .HasMaxLength(500);

    // Indexes
    builder.HasIndex(t => t.SourceCustomerId);
    builder.HasIndex(t => t.DestinationCustomerId);
    builder.HasIndex(t => t.CreatedAt);

    // Ignore domain events
    builder.Ignore(t => t.DomainEvents);
  }
}