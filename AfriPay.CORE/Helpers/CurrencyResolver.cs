using AfriPay.CORE.Enums;

namespace AfriPay.CORE.Helpers
{
    /// <summary>
    /// Central helper to resolve a default currency based on identity type or country.
    /// This avoids scattering hard-coded "NGN"/"GHS"/"KES" strings and keeps
    /// onboarding/account creation logic consistent.
    /// </summary>
    public static class CurrencyResolver
    {
        public static string FromIdentityType(IdentityType identityType) => identityType switch
        {
            IdentityType.BVN => "NGN",              // Nigeria
            IdentityType.GhanaCard => "GHS",        // Ghana
            IdentityType.KenyaNationalID => "KES",  // Kenya
            _ => "NGN"                               // Sensible default for now
        };

        public static string FromCountry(Country country) => country switch
        {
            Country.Nigeria => "NGN",
            Country.Ghana => "GHS",
            Country.Kenya => "KES",
            _ => "NGN"
        };
    }
}
