using System;
using System.Linq;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Kenya National ID
    /// </summary>
    public class KenyaNationalID : IdentityNumber
    {
        private KenyaNationalID() : base() { } // EF Core

        private KenyaNationalID(string value) : base(value, "KE")
        {
            Validate();
        }

        public static KenyaNationalID Create(string value) => new(value);

        protected override void Validate()
        {
            // Kenya National ID is typically 7-8 digits
            if (Value.Length < 7 || Value.Length > 9)
                throw new ArgumentException("Kenya National ID must be between 7 and 9 digits", nameof(Value));

            if (!Value.All(char.IsDigit))
                throw new ArgumentException("Kenya National ID must contain only digits", nameof(Value));
        }
    }
}
