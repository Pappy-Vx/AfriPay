using MediatR;

namespace AfriPay.APP.Customers.Queries.CheckUserTagAvailability;

public record CheckUserTagAvailabilityQuery(string UserTag) : IRequest<UserTagAvailabilityResponse>;

public record UserTagAvailabilityResponse(
    string UserTag,
    bool IsAvailable,
    string? SuggestedAlternative = null);