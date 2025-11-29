using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    public record AccountNumber
    {
        public string Value { get; init; }

        private AccountNumber() { }
        private AccountNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Account number cannot be empty", nameof(value));

            if (value.Length != 10)
                throw new ArgumentException("Account number must be 10 digits", nameof(value));

            Value = value;
        }

        public static AccountNumber Create(string value) => new(value);
        public override string ToString() => Value;
    }

}
