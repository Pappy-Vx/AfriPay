using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Customers.Queries.GetCustomerByUserTag;

/// <summary>
/// Query to retrieve customer details by UserTag.
/// </summary>
public record GetCustomerByUserTagQuery(string UserTag) : IRequest<Result<CustomerDetailsResponse>>;

/// <summary>
/// DTO representing basic customer details returned when querying by UserTag.
/// </summary>
public record CustomerDetailsResponse
{
    /// <summary>
    /// Unique customer identifier (GUID).
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// Primary account identifier for the customer, if an account exists.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Currency of the primary account, if an account exists.
    /// </summary>
    public string? AccountCurrency { get; init; }

    /// <summary>
    /// The customer's UserTag (always prefixed with '@').
    /// </summary>
    public string UserTag { get; init; } = string.Empty;

    /// <summary>
    /// Customer first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Customer last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Customer email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the customer account is active.
    /// </summary>
    public bool IsActive { get; init; }
}
