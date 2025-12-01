namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Value object representing personal information
    /// </summary>
    public class PersonalInfo
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? MiddleName { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public string Gender { get; private set; }

        /// <summary>
        /// Computed property for full name
        /// </summary>
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        /// <summary>
        /// Computed property for age
        /// </summary>
        public int Age
        {
            get
            {
                var today = DateTime.UtcNow;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        private PersonalInfo() { } // For EF Core

        public PersonalInfo(
            string firstName,
            string lastName,
            string? middleName,
            DateTime dateOfBirth,
            string gender = "OTHER")
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            if (dateOfBirth == default)
                throw new ArgumentException("Date of birth must be provided", nameof(dateOfBirth));

            if (dateOfBirth > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));

            // Validate minimum age (18 years)
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
            if (age < 18)
                throw new ArgumentException("Customer must be at least 18 years old", nameof(dateOfBirth));

            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty", nameof(gender));

            if (gender.ToUpper() != "M" && gender.ToUpper() != "F" && gender.ToUpper() != "OTHER")
                throw new ArgumentException("Gender must be M, F, or OTHER", nameof(gender));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
            DateOfBirth = dateOfBirth.Date;
            Gender = gender.ToUpper();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not PersonalInfo other) return false;

            return FirstName == other.FirstName &&
                   LastName == other.LastName &&
                   MiddleName == other.MiddleName &&
                   DateOfBirth == other.DateOfBirth &&
                   Gender == other.Gender;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, MiddleName, DateOfBirth, Gender);
        }
    }
}
