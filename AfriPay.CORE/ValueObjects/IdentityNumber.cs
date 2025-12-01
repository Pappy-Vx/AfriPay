namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Base class for identity numbers across different countries
    /// </summary>
    public abstract class IdentityNumber
    {
        public string Value { get; protected set; }
        public string Country { get; protected set; }

        protected IdentityNumber() { } // For EF Core

        protected IdentityNumber(string value, string country)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Identity number cannot be empty", nameof(value));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be empty", nameof(country));

            Value = value.Trim();
            Country = country.ToUpper();
        }

        protected abstract void Validate();

        public override bool Equals(object? obj)
        {
            if (obj is not IdentityNumber other) return false;
            return Value == other.Value && Country == other.Country;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Country);
        }

        public override string ToString() => Value;
    }
}
