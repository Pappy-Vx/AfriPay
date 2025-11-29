using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    public record CustomerReference
    {
        public string Value { get; init; }

        private CustomerReference() { }
        private CustomerReference(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Customer reference cannot be empty", nameof(value));

            Value = value;
        }

        public static CustomerReference Create() => new($"CUST-{Guid.NewGuid():N}".ToUpper());
        public static CustomerReference Create(string value) => new(value);
        public override string ToString() => Value;
    }
}
