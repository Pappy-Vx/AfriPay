using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// National ID for Kenya
    /// </summary>
    public class KenyaNationalId : IdentityNumber
    {
        private KenyaNationalId() { } // For EF Core

        public KenyaNationalId(string value) : base(value, "KE")
        {
            Validate();
        }

        protected override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("Kenya National ID cannot be empty");

            // Kenya National ID format: 7-8 digits
            if (!Regex.IsMatch(Value, @"^\d{7,8}$"))
                throw new ArgumentException("Kenya National ID must be 7 or 8 digits");
        }

        public static KenyaNationalId Create(string value) => new KenyaNationalId(value);
    }
}
