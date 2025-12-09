using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AfriPay.APP.Customers.Queries.GetCustomerByUserTag;

/// <summary>
/// Handler for <see cref="GetCustomerByUserTagQuery"/>.
/// Looks up a customer by their UserTag and returns basic profile details.
/// </summary>
public class GetCustomerByUserTagQueryHandler
    : IRequestHandler<GetCustomerByUserTagQuery, Result<CustomerDetailsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCustomerByUserTagQueryHandler> _logger;

    public GetCustomerByUserTagQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCustomerByUserTagQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CustomerDetailsResponse>> Handle(
        GetCustomerByUserTagQuery request,
        CancellationToken cancellationToken)
    {
        // Validate and normalize the UserTag using the value object rules.
        var userTagResult = UserTag.Create(request.UserTag);
        if (!userTagResult.IsSuccess)
        {
            _logger.LogWarning(
                "Invalid UserTag format: {UserTag}. Error: {Error}",
                request.UserTag,
                userTagResult.Error);

            return Result.Failure<CustomerDetailsResponse>(userTagResult.Error);
        }

        var normalizedTagValue = userTagResult.Value.Value;

        // Repository will apply its own normalization but we pass the cleaned value.
        var customer = await _unitOfWork.Customers.GetByUserTagAsync(
            normalizedTagValue,
            cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning(
                "Customer not found for UserTag: {UserTag}",
                normalizedTagValue);

            return Result.Failure<CustomerDetailsResponse>("Customer not found");
        }

        // Try to resolve a primary account for this customer (fallback to first account if none flagged primary).
        var accounts = await _unitOfWork.Accounts.GetByCustomerIdAsync(customer.CustomerId, cancellationToken);
        var primaryAccount = accounts.FirstOrDefault(a => a.PrimaryAccountInfo != null)
                            ?? accounts.FirstOrDefault();

        var response = new CustomerDetailsResponse
        {
            CustomerId = customer.CustomerId.Value,
            AccountId = primaryAccount?.AccountId.Value,
            AccountCurrency = primaryAccount?.Balance.Currency,
            UserTag = customer.UserTag?.DisplayTag ?? string.Empty,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.ContactInfo?.Email ?? customer.Email,
            PhoneNumber = customer.ContactInfo?.PhoneNumber ?? customer.PhoneNumber,
            IsActive = customer.IsActive,
        };

        return Result.Success(response);
    }
}
