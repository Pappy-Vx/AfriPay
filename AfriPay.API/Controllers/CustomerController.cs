using AfriPay.APP.Customers.Commands.SetUserTag;
using AfriPay.APP.Customers.Queries.CheckUserTagAvailability;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for managing customer-related operations. This includes checking user tag availability and setting user tags.
    /// All endpoints require proper authentication if configured in the API.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="mediator">The MediatR mediator for sending commands and queries.</param>
        /// <param name="logger">The logger for the controller.</param>
        public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if a specified user tag is available for assignment.
        /// </summary>
        /// <remarks>
        /// This endpoint verifies the availability of a user tag. User tags must be unique across the system.
        ///
        /// **Path Parameter:**
        /// - tag: The user tag to check (required, string). Should be in the format '@username' or just 'username'.
        ///
        /// **Validation Notes:**
        /// - The tag is trimmed and validated for proper format.
        /// - Returns whether the tag is available or already in use.
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/customer/usertag/check/@johndoe
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "isAvailable": true,
        ///   "tag": "@johndoe"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Availability status returned.
        /// - 400 Bad Request: Invalid tag format.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="tag">The user tag to check availability for.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the availability response.</returns>
        /// <response code="200">User tag availability checked successfully</response>
        /// <response code="400">Bad request - invalid tag</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("usertag/check/{tag}")]
        [ProducesResponseType(typeof(UserTagAvailabilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Check if a UserTag is available",
            OperationId = "CheckUserTagAvailability",
            Tags = new[] { "Customer" }
        )]
        public async Task<ActionResult<UserTagAvailabilityResponse>> CheckUserTagAvailability(
            string tag,
            CancellationToken cancellationToken)
        {
            var query = new CheckUserTagAvailabilityQuery(tag);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Sets a user tag for a customer (one-time operation after onboarding).
        /// </summary>
        /// <remarks>
        /// This endpoint allows setting a unique user tag for a customer. This can only be done once per customer.
        ///
        /// **Path Parameter:**
        /// - customerId: The unique GUID identifier for the customer (required).
        ///
        /// **Request Body (SetUserTagRequest):**
        /// - UserTag: The desired user tag (required, string). Should include '@' prefix (e.g., "@johndoe").
        ///
        /// **Validation Notes:**
        /// - User tag must be unique and available.
        /// - Can only be set once; subsequent attempts will fail.
        /// - Invalid formats or unavailable tags will result in 400 Bad Request.
        ///
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/customer/{customerId}/usertag
        /// {
        ///   "userTag": "@johndoe"
        /// }
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "message": "UserTag '@johndoe' set successfully"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: User tag set successfully.
        /// - 400 Bad Request: Invalid request, tag unavailable, or already set.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="customerId">The GUID of the customer to set the user tag for.</param>
        /// <param name="request">The user tag request details.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns success message on completion, or error details.</returns>
        /// <response code="200">User tag set successfully</response>
        /// <response code="400">Bad request - validation errors or tag unavailable</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpPost("{customerId:guid}/usertag")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Set UserTag for a customer (one-time, after onboarding)",
            OperationId = "SetUserTag",
            Tags = new[] { "Customer" }
        )]
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
}