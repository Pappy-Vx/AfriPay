using System;
using System.Linq;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Ghana Card National ID
    /// </summary>
    public class GhanaCard : IdentityNumber
    {
        private GhanaCard() : base() { } // EF Core

        private GhanaCard(string value) : base(value, "GH")
        {
            Validate();
        }

        public static GhanaCard Create(string value) => new(value);

        protected override void Validate()
        {
            // Ghana Card format: GHA-XXXXXXXXX-X (15 characters with hyphens)
            if (Value.Length < 10 || Value.Length > 20)
                throw new ArgumentException("Ghana Card must be between 10 and 20 characters", nameof(Value));

            // Basic alphanumeric validation
            if (!Value.All(c => char.IsLetterOrDigit(c) || c == '-'))
                throw new ArgumentException("Ghana Card must contain only letters, digits, and hyphens", nameof(Value));
        }
    }
}
