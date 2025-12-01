using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Bank Verification Number for Nigeria
    /// </summary>
    public class BvnNumber : IdentityNumber
    {
        private BvnNumber() { } // For EF Core

        public BvnNumber(string value) : base(value, "NG")
        {
            Validate();
        }

        protected override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("BVN cannot be empty");

            if (Value.Length != 11)
                throw new ArgumentException("BVN must be exactly 11 digits");

            if (!Regex.IsMatch(Value, @"^\d{11}$"))
                throw new ArgumentException("BVN must contain only digits");
        }

        public static BvnNumber Create(string value) => new BvnNumber(value);
    }
}
