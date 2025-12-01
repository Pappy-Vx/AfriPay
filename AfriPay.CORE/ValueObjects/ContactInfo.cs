using System.Text.RegularExpressions;

namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Value object representing contact information
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string? AlternativePhoneNumber { get; private set; }

        private ContactInfo() { } // For EF Core

        public ContactInfo(
            string email,
            string phoneNumber,
            string? alternativePhoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid email format", nameof(email));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

            if (!IsValidPhoneNumber(phoneNumber))
                throw new ArgumentException("Invalid phone number format. Must be in international format (e.g., +2348012345678)", nameof(phoneNumber));

            if (!string.IsNullOrWhiteSpace(alternativePhoneNumber) && !IsValidPhoneNumber(alternativePhoneNumber))
                throw new ArgumentException("Invalid alternative phone number format", nameof(alternativePhoneNumber));

            Email = email.Trim().ToLowerInvariant();
            PhoneNumber = phoneNumber.Trim();
            AlternativePhoneNumber = string.IsNullOrWhiteSpace(alternativePhoneNumber) ? null : alternativePhoneNumber.Trim();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple email validation
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Must start with + and have 10-15 digits (international format)
            var phoneRegex = new Regex(@"^\+[1-9]\d{9,14}$");
            return phoneRegex.IsMatch(phoneNumber);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ContactInfo other) return false;

            return Email == other.Email &&
                   PhoneNumber == other.PhoneNumber &&
                   AlternativePhoneNumber == other.AlternativePhoneNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Email, PhoneNumber, AlternativePhoneNumber);
        }
    }
}
