using AfriPay.CORE.ValueObjects.AfriPay.CORE.ValueObjects;
using System;
using System.Linq;

using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Kenya National ID
    /// </summary>
    public class KenyaNationalID : IdentityNumber
    {
        protected KenyaNationalID() { } // For EF Core

        public KenyaNationalID(string value) : base(value, "KE")
        {
            // Base ctor calls Validate
        }

        protected override void Validate()
        {
            base.Validate(); // Call base for common rules

            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("Kenya National ID cannot be empty");
            if (Value.Length < 7 || Value.Length > 9)
                throw new ArgumentException("Kenya National ID must be between 7 and 9 digits (typically 8 for modern IDs)");
            if (!Regex.IsMatch(Value, @"^\d+$"))
                throw new ArgumentException("Kenya National ID must contain only digits");
        }

        public static KenyaNationalID Create(string value) => new KenyaNationalID(value);
    }
}

//namespace AfriPay.CORE.ValueObjects
//{
//    /// <summary>
//    /// Kenya National ID
//    /// </summary>
//    public class KenyaNationalID : IdentityNumber
//    {
//        private KenyaNationalID() : base() { } // EF Core

//        private KenyaNationalID(string value) : base(value, "KE")
//        {
//            Validate();
//        }

//        public static KenyaNationalID Create(string value) => new(value);

//        protected override void Validate()
//        {
//            // Kenya National ID is typically 7-8 digits
//            if (Value.Length < 7 || Value.Length > 9)
//                throw new ArgumentException("Kenya National ID must be between 7 and 9 digits", nameof(Value));

//            if (!Value.All(char.IsDigit))
//                throw new ArgumentException("Kenya National ID must contain only digits", nameof(Value));
//        }
//    }
//}
