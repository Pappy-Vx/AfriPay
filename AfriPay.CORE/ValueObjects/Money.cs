using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "NGN";

        // Parameterless constructor for EF Core
        private Money() { }

        public Money(decimal amount, string currency = "NGN")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Amount = amount;
            Currency = currency ?? "NGN";
        }

        public static Money Zero => new(0);

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies"); return new Money(a.Amount - b.Amount, a.Currency);
        }
        public static Money ZeroWithCurrency(string currency) => new(0, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return new Money(Amount - other.Amount, Currency);
        }
        public override string ToString() => $"{Currency} {Amount:N2}";

    }

}
