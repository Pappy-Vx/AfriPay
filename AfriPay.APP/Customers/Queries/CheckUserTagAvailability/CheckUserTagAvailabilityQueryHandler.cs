using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;

namespace AfriPay.APP.Customers.Queries.CheckUserTagAvailability;

public class CheckUserTagAvailabilityQueryHandler
    : IRequestHandler<CheckUserTagAvailabilityQuery, UserTagAvailabilityResponse>
{
  private readonly IUnitOfWork _unitOfWork;

  public CheckUserTagAvailabilityQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<UserTagAvailabilityResponse> Handle(
      CheckUserTagAvailabilityQuery request,
      CancellationToken cancellationToken)
  {
    // Validate format first
    var tagResult = UserTag.Create(request.UserTag);
    if (!tagResult.IsSuccess)
      return new UserTagAvailabilityResponse(request.UserTag, false, null);

    var normalizedTag = tagResult.Value.Value;

    var isAvailable = await _unitOfWork.Customers.IsUserTagAvailableAsync(normalizedTag, cancellationToken);

    string? suggestion = null;
    if (!isAvailable)
    {
      // Generate suggestion by adding random numbers
      var random = new Random();
      for (int i = 0; i < 5; i++)
      {
        var suggestedTag = $"{normalizedTag}{random.Next(1, 999)}";
        if (await _unitOfWork.Customers.IsUserTagAvailableAsync(suggestedTag, cancellationToken))
        {
          suggestion = $"@{suggestedTag}";
          break;
        }
      }
    }

    return new UserTagAvailabilityResponse($"@{normalizedTag}", isAvailable, suggestion);
  }
}