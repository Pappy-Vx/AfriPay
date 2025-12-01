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
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name contains invalid characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name contains invalid characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.BVN)
                .NotEmpty().WithMessage("BVN is required")
                .Length(11).WithMessage("BVN must be exactly 11 digits")
                .Matches(@"^\d{11}$").WithMessage("BVN must contain only digits");
        }
    }
}
