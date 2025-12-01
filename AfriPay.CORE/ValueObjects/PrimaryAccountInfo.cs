namespace AfriPay.CORE.ValueObjects
{
    /// <summary>
    /// Value object representing primary account information for PAPS integration
    /// </summary>
    public class PrimaryAccountInfo
    {
        public string PrimaryAccountNumber { get; private set; }
        public string PrimaryAccountName { get; private set; }
        public string PrimaryBankCode { get; private set; }

        private PrimaryAccountInfo() { } // For EF Core

        public PrimaryAccountInfo(
            string primaryAccountNumber,
            string primaryAccountName,
            string primaryBankCode)
        {
            if (string.IsNullOrWhiteSpace(primaryAccountNumber))
                throw new ArgumentException("Primary account number cannot be empty", nameof(primaryAccountNumber));

            if (string.IsNullOrWhiteSpace(primaryAccountName))
                throw new ArgumentException("Primary account name cannot be empty", nameof(primaryAccountName));

            if (string.IsNullOrWhiteSpace(primaryBankCode))
                throw new ArgumentException("Primary bank code cannot be empty", nameof(primaryBankCode));

            // Validate account number format (10 digits for Nigerian banks)
            if (primaryAccountNumber.Length != 10 || !primaryAccountNumber.All(char.IsDigit))
                throw new ArgumentException("Primary account number must be 10 digits", nameof(primaryAccountNumber));

            PrimaryAccountNumber = primaryAccountNumber;
            PrimaryAccountName = primaryAccountName.Trim();
            PrimaryBankCode = primaryBankCode.Trim();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not PrimaryAccountInfo other) return false;

            return PrimaryAccountNumber == other.PrimaryAccountNumber &&
                   PrimaryAccountName == other.PrimaryAccountName &&
                   PrimaryBankCode == other.PrimaryBankCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PrimaryAccountNumber, PrimaryAccountName, PrimaryBankCode);
        }
    }
}
