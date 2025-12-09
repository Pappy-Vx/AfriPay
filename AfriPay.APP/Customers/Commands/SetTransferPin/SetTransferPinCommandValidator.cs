using FluentValidation;

namespace AfriPay.APP.Customers.Commands.SetTransferPin;

public class SetTransferPinCommandValidator : AbstractValidator<SetTransferPinCommand>
{
    public SetTransferPinCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("PIN is required")
            .MinimumLength(4).WithMessage("PIN must be at least 4 characters")
            .MaximumLength(12).WithMessage("PIN cannot exceed 12 characters");

        RuleFor(x => x.ConfirmPin)
            .NotEmpty().WithMessage("Confirm PIN is required")
            .Equal(x => x.Pin).WithMessage("PIN and Confirm PIN do not match");
    }
}
