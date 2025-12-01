using FluentValidation;

namespace AfriPay.APP.Onboarding.Commands.StartOnboarding
{
    /// <summary>
    /// Validator for StartOnboardingCommand
    /// </summary>
    public class StartOnboardingCommandValidator : AbstractValidator<StartOnboardingCommand>
    {
        public StartOnboardingCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .LessThan(DateTime.UtcNow.AddYears(-18))
                .WithMessage("Must be at least 18 years old");

            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("Identity number is required");

            RuleFor(x => x.SelfieUrl)
                .NotEmpty().WithMessage("Selfie is required")
                .Must(BeValidUrl).WithMessage("Invalid selfie URL");

            RuleFor(x => x.ConsentGiven)
                .Equal(true).WithMessage("Consent is required");
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
