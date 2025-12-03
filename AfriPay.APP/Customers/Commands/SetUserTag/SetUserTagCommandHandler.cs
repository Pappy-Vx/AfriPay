using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Customers.Commands.SetUserTag;

public class SetUserTagCommandHandler : IRequestHandler<SetUserTagCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetUserTagCommandHandler> _logger;

    public SetUserTagCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SetUserTagCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SetUserTagCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting UserTag for customer: {CustomerId}", request.CustomerId);

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

        // 3. Check if tag is available (skip if customer is updating to same tag)
        if (customer.UserTag == null || customer.UserTag.NormalizedTag != userTag.NormalizedTag)
        {
            var isAvailable = await _unitOfWork.Customers.IsUserTagAvailableAsync(userTag.Value, cancellationToken);
            if (!isAvailable)
                return Result.Failure($"UserTag '{userTag.DisplayTag}' is already taken. Try another one.");
        }

        // 4. Set UserTag (allows change within 7 days of account creation)
        var setResult = customer.SetUserTag(userTag);
        if (!setResult.IsSuccess)
            return setResult;

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UserTag '{UserTag}' set for customer: {CustomerId}",
            userTag.DisplayTag, request.CustomerId);

        return Result.Success();
    }
}