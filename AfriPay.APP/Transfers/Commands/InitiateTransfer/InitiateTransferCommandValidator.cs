using FluentValidation;

namespace AfriPay.APP.Transfers.Commands.InitiateTransfer;

public class InitiateTransferCommandValidator : AbstractValidator<InitiateTransferCommand>
{
    public InitiateTransferCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account is required");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account is required")
            .NotEqual(x => x.SourceAccountId).WithMessage("Cannot transfer to the same account");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(10000000).WithMessage("Amount exceeds maximum transfer limit");

        When(x => !string.IsNullOrWhiteSpace(x.Currency), () =>
        {
            RuleFor(x => x.Currency)
                .Length(3).WithMessage("Currency must be 3 characters (e.g., NGN, USD)");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        });
    }
}
