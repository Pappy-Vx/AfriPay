using AfriPay.CORE.Enums;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string TransactionReference { get; private set; } = string.Empty;
    public AccountId AccountId { get; private set; }
    public CustomerId CustomerId { get; private set; }

    public TransactionDirection Direction { get; private set; }
    public Money Amount { get; private set; }  // This needs OwnsOne configuration
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }

    public TransferId? TransferId { get; private set; }
    public string Narration { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Transaction()
    {
        // EF Core parameterless constructor
        AccountId = AccountId.Create(Guid.Empty);
        CustomerId = CustomerId.Create(Guid.Empty);
        Amount = new Money(0);
    }

    public static Transaction CreateDebit(
        AccountId accountId,
        CustomerId customerId,
        Money amount,
        decimal balanceBefore,
        string narration,
        TransferId? transferId = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionReference = GenerateReference(),
            AccountId = accountId,
            CustomerId = customerId,
            Direction = TransactionDirection.Debit,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceBefore - amount.Amount,
            TransferId = transferId,
            Narration = narration,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateCredit(
        AccountId accountId,
        CustomerId customerId,
        Money amount,
        decimal balanceBefore,
        string narration,
        TransferId? transferId = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionReference = GenerateReference(),
            AccountId = accountId,
            CustomerId = customerId,
            Direction = TransactionDirection.Credit,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceBefore + amount.Amount,
            TransferId = transferId,
            Narration = narration,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateReference()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}