using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Bank Verification Number (BVN) for Nigeria
    /// </summary>
    public class BVN : IdentityNumber
    {
        private BVN() : base() { } // EF Core

        private BVN(string value) : base(value, "NG")
        {
            Validate();
        }

        public static BVN Create(string value) => new(value);

        protected override void Validate()
        {
            if (Value.Length != 11)
                throw new ArgumentException("BVN must be 11 digits", nameof(Value));

            if (!Value.All(char.IsDigit))
                throw new ArgumentException("BVN must contain only digits", nameof(Value));
        }
    }
}
