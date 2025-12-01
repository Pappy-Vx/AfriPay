using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
using System.Text.RegularExpressions;

using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Ghana Card (National ID) for Ghana
    /// </summary>
    public class GhanaCardNumber : IdentityNumber
    {
        protected GhanaCardNumber() { } // For EF Core

        public GhanaCardNumber(string value) : base(value, "GH")
        {
            // Base ctor calls Validate
        }

        protected override void Validate()
        {
            base.Validate(); // Call base for common rules

            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("Ghana Card number cannot be empty");
            if (!Regex.IsMatch(Value, @"^GHA-\d{9}-\d{1}$"))
                throw new ArgumentException("Ghana Card number must be in format GHA-XXXXXXXXX-X (9 digits after GHA-, then 1 digit)");
        }

        public static GhanaCardNumber Create(string value) => new GhanaCardNumber(value);
    }
}

//namespace AfriPay.CORE.ValueObjects
//{
//    /// <summary>
//    /// Ghana Card (National ID) for Ghana
//    /// </summary>
//    public class GhanaCardNumber : IdentityNumber
//    {
//        private GhanaCardNumber() { } // For EF Core

//        public GhanaCardNumber(string value) : base(value, "GH")
//        {
//            Validate();
//        }

//        protected override void Validate()
//        {
//            if (string.IsNullOrWhiteSpace(Value))
//                throw new ArgumentException("Ghana Card number cannot be empty");

//            // Ghana Card format: GHA-XXXXXXXXX-X (example)
//            if (!Regex.IsMatch(Value, @"^GHA-\d{9}-\d{1}$"))
//                throw new ArgumentException("Ghana Card number must be in format GHA-XXXXXXXXX-X");
//        }

//        public static GhanaCardNumber Create(string value) => new GhanaCardNumber(value);
//    }
//}
