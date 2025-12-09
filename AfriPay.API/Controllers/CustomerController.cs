using AfriPay.APP.Customers.Commands.SetUserTag;
using AfriPay.APP.Customers.Queries.CheckUserTagAvailability;
using AfriPay.APP.Customers.Queries.GetCustomerByUserTag;
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
        /// Retrieves basic customer details by UserTag.
        /// </summary>
        /// <remarks>
        /// This endpoint looks up a customer profile using their UserTag.
        ///
        /// **Path Parameter:**
        /// - tag: The user tag to look up (required, string). Can be provided with or without the '@' prefix.
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/customer/usertag/@johndoe
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "userTag": "@johndoe",
        ///   "firstName": "John",
        ///   "lastName": "Doe",
        ///   "email": "john.doe@example.com",
        ///   "phoneNumber": "+2348012345678",
        ///   "isActive": true
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Customer details returned.
        /// - 400 Bad Request: Invalid UserTag format.
        /// - 404 Not Found: No customer found for the given UserTag.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="tag">The UserTag to retrieve customer details for.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Customer details when found, or error information.</returns>
        /// <response code="200">Customer details retrieved successfully</response>
        /// <response code="400">Bad request - invalid UserTag format</response>
        /// <response code="404">Customer not found for the given UserTag</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("usertag/{tag}")]
        [ProducesResponseType(typeof(CustomerDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get customer details by UserTag",
            OperationId = "GetCustomerByUserTag",
            Tags = new[] { "Customer" }
        )]
        public async Task<ActionResult<CustomerDetailsResponse>> GetCustomerByUserTag(
            string tag,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerByUserTagQuery(tag);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                if (string.Equals(result.Error, "Customer not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.Error,
                        Status = StatusCodes.Status404NotFound,
                        Instance = HttpContext.Request.Path
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid UserTag",
                    Detail = result.Error,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Sets a user tag and password for a customer (account activation after onboarding).
        /// </summary>
        /// <remarks>
        /// This endpoint allows setting a unique user tag and password for a customer after onboarding.
        /// This activates the customer's account and allows them to login.
        ///
        /// **Path Parameter:**
        /// - customerId: The unique GUID identifier for the customer (required).
        ///
        /// **Request Body (SetUserTagRequest):**
        /// - UserTag: The desired user tag (required, string). Should include '@' prefix (e.g., "@johndoe").
        /// - Password: The customer's password (required, min 8 chars, must contain uppercase, lowercase, number, special char).
        /// - ConfirmPassword: Must match Password.
        ///
        /// **Validation Notes:**
        /// - User tag must be unique and available.
        /// - Can only be done once per customer (account activation).
        /// - Password must meet security requirements.
        ///
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/customer/{customerId}/usertag
        /// {
        ///   "userTag": "@johndoe",
        ///   "password": "SecurePassword123!",
        ///   "confirmPassword": "SecurePassword123!"
        /// }
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "message": "Account activated successfully. UserTag '@johndoe' set."
        /// }
        /// ```
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
            Summary = "Set UserTag and Password for a customer (account activation after onboarding)",
            OperationId = "SetUserTag",
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> SetUserTag(
            Guid customerId,
            [FromBody] SetUserTagRequest request,
            CancellationToken cancellationToken)
        {
            var command = new SetUserTagCommand(
                customerId,
                request.UserTag,
                request.Password,
                request.ConfirmPassword);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = $"Account activated successfully. UserTag '@{request.UserTag.TrimStart('@')}' set." });
        }
    }

    /// <summary>
    /// Request to set UserTag and Password (account activation)
    /// </summary>
    public record SetUserTagRequest(
        /// <summary>
        /// The desired user tag (e.g., "@johndoe")
        /// </summary>
        string UserTag,

        /// <summary>
        /// Password (min 8 chars, uppercase, lowercase, number, special char required)
        /// </summary>
        string Password,

        /// <summary>
        /// Must match Password
        /// </summary>
        string ConfirmPassword
    );
}