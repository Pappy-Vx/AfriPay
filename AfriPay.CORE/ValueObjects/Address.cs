namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Value object representing a physical address with multi-country support
    /// </summary>
    public class Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string? PostalCode { get; private set; }

        /// <summary>
        /// Full formatted address
        /// </summary>
        public string FullAddress => string.IsNullOrWhiteSpace(PostalCode)
            ? $"{Street}, {City}, {State}, {Country}"
            : $"{Street}, {City}, {State} {PostalCode}, {Country}";

        private Address() { } // For EF Core

        public Address(
            string street,
            string city,
            string state,
            string country,
            string? postalCode = null)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be empty", nameof(street));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State cannot be empty", nameof(state));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be empty", nameof(country));

            // Validate country code (ISO 3166-1 alpha-2)
            var validCountries = new[] { "NG", "GH", "KE", "ZA", "UG", "TZ", "RW" };
            if (!validCountries.Contains(country.ToUpper()))
                throw new ArgumentException($"Country code must be one of: {string.Join(", ", validCountries)}", nameof(country));

            Street = street.Trim();
            City = city.Trim();
            State = state.Trim();
            Country = country.ToUpper();
            PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Address other) return false;

            return Street == other.Street &&
                   City == other.City &&
                   State == other.State &&
                   Country == other.Country &&
                   PostalCode == other.PostalCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, City, State, Country, PostalCode);
        }
    }
}
