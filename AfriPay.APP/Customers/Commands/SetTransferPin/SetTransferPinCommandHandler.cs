using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Customers.Commands.SetTransferPin;

public class SetTransferPinCommandHandler : IRequestHandler<SetTransferPinCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SetTransferPinCommandHandler> _logger;

    public SetTransferPinCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<SetTransferPinCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(SetTransferPinCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting transfer PIN for customer: {CustomerId}", request.CustomerId);

        if (!string.Equals(request.Pin, request.ConfirmPin, StringComparison.Ordinal))
        {
            return Result.Failure("PIN and Confirm PIN do not match");
        }

        var customer = await _unitOfWork.Customers.GetByIdAsync(
            CustomerId.Create(request.CustomerId),
            cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer not found when setting transfer PIN: {CustomerId}", request.CustomerId);
            return Result.Failure("Customer not found");
        }

        // Hash and set transfer PIN
        var pinHash = _passwordHasher.HashPassword(request.Pin);
        customer.SetTransferPin(pinHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transfer PIN set for customer: {CustomerId}", request.CustomerId);

        return Result.Success();
    }
}
