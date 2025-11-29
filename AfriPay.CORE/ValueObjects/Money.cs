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
        public string Currency { get; init; }

        private Money() { }

        public Money(decimal amount, string currency = "NGN")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Amount = amount;
            Currency = currency ?? "NGN";
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public override string ToString() => $"{Currency} {Amount:N2}";
    }

}
