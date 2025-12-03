using AfriPay.APP.Customers.Commands.SetUserTag;
using AfriPay.APP.Customers.Queries.CheckUserTagAvailability;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly ILogger<CustomerController> _logger;

  public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  /// <summary>
  /// Check if a UserTag is available
  /// </summary>
  [HttpGet("usertag/check/{tag}")]
  public async Task<ActionResult<UserTagAvailabilityResponse>> CheckUserTagAvailability(
      string tag,
      CancellationToken cancellationToken)
  {
    var query = new CheckUserTagAvailabilityQuery(tag);
    var result = await _mediator.Send(query, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Set UserTag for a customer (one-time, after onboarding)
  /// </summary>
  [HttpPost("{customerId:guid}/usertag")]
  public async Task<IActionResult> SetUserTag(
      Guid customerId,
      [FromBody] SetUserTagRequest request,
      CancellationToken cancellationToken)
  {
    //commmand to
    var command = new SetUserTagCommand(customerId, request.UserTag);
    var result = await _mediator.Send(command, cancellationToken);

    if (!result.IsSuccess)
      return BadRequest(new { error = result.Error });

    return Ok(new { message = $"UserTag '@{request.UserTag.TrimStart('@')}' set successfully" });
  }
}

public record SetUserTagRequest(string UserTag);