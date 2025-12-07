using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Customers.Commands.SetUserTag;

public class SetUserTagCommandHandler : IRequestHandler<SetUserTagCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SetUserTagCommandHandler> _logger;

    public SetUserTagCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<SetUserTagCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result> Handle(SetUserTagCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting UserTag and Password for customer: {CustomerId}", request.CustomerId);

        // 1. Validate UserTag format
        var userTagResult = UserTag.Create(request.UserTag);
        if (!userTagResult.IsSuccess)
            return Result.Failure(userTagResult.Error);

        var userTag = userTagResult.Value;

        // 2. Get customer
        var customer = await _unitOfWork.Customers.GetByIdAsync(
            CustomerId.Create(request.CustomerId), cancellationToken);

        if (customer == null)
            return Result.Failure("Customer not found");

        // 3. Check if customer already has a password set (account already activated)
        if (!string.IsNullOrEmpty(customer.PasswordHash))
            return Result.Failure("Account is already activated. Use the update profile endpoint to change your password.");

        // 4. Check if tag is available (skip if customer is updating to same tag)
        if (customer.UserTag == null || customer.UserTag.NormalizedTag != userTag.NormalizedTag)
        {
            var isAvailable = await _unitOfWork.Customers.IsUserTagAvailableAsync(userTag.Value, cancellationToken);
            if (!isAvailable)
                return Result.Failure($"UserTag '{userTag.DisplayTag}' is already taken. Try another one.");
        }

        // 5. Set UserTag
        var setResult = customer.SetUserTag(userTag);
        if (!setResult.IsSuccess)
            return setResult;

        // 6. Hash and set password
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        customer.SetPassword(passwordHash);

        // 7. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UserTag '{UserTag}' and password set for customer: {CustomerId}",
            userTag.DisplayTag, request.CustomerId);

        return Result.Success();
    }
}