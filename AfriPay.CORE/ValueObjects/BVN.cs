using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    //public record BVN
    //{
    //    public string Value { get;}

    //    private BVN() { }
    //    private BVN(string value)
    //    {
    //        if (string.IsNullOrWhiteSpace(value))
    //            throw new ArgumentException("BVN cannot be empty", nameof(value));

    //        if (value.Length != 11)
    //            throw new ArgumentException("BVN must be 11 digits", nameof(value));

    //        if (!value.All(char.IsDigit))
    //            throw new ArgumentException("BVN must contain only digits", nameof(value));

    //        Value = value;
    //    }

    //    public static BVN Create(string value) => new(value);
    //    public override string ToString() => Value;
    //}
    public record BVN
    {
        public string Value { get; }
        public BVN(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 11)
                throw new ArgumentException("BVN must be 11 digits");
            if (!value.All(char.IsDigit))
                throw new ArgumentException("BVN must contain only digits");
            Value = value;
        }
        // EF Core needs this
        private BVN() : this(string.Empty) { }
        public static BVN Create(string value) => new(value);
        public static implicit operator string(BVN number) => number.Value;
        public static explicit operator BVN(string value) => new(value);
        public override string ToString() => Value;
    }
}
