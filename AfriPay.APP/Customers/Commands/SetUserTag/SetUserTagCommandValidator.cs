using FluentValidation;

namespace AfriPay.APP.Customers.Commands.SetUserTag;

public class SetUserTagCommandValidator : AbstractValidator<SetUserTagCommand>
{
  public SetUserTagCommandValidator()
  {
    RuleFor(x => x.CustomerId)
        .NotEmpty().WithMessage("Customer ID is required");

    RuleFor(x => x.UserTag)
        .NotEmpty().WithMessage("UserTag is required")
        .MinimumLength(3).WithMessage("UserTag must be at least 3 characters")
        .MaximumLength(30).WithMessage("UserTag cannot exceed 30 characters")
        .Matches(@"^@?[a-zA-Z][a-zA-Z0-9_]*$")
        .WithMessage("UserTag must start with a letter and contain only letters, numbers, and underscores");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required")
        .MinimumLength(8).WithMessage("Password must be at least 8 characters")
        .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
        .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
        .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
        .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

    RuleFor(x => x.ConfirmPassword)
        .NotEmpty().WithMessage("Confirm password is required")
        .Equal(x => x.Password).WithMessage("Passwords do not match");
  }
}