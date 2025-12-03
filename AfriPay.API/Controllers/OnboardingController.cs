using AfriPay.APP.Onboarding.Commands.StartOnboarding;
using AfriPay.APP.Onboarding.Queries.GetOnboardingStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for managing customer onboarding processes. This includes starting the onboarding and checking status.
    /// All endpoints require proper authentication if configured in the API.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OnboardingController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnboardingController"/> class.
        /// </summary>
        /// <param name="mediator">The MediatR mediator for sending commands and queries.</param>
        /// <param name="logger">The logger for the controller.</param>
        public OnboardingController(
            IMediator mediator,
            ILogger<OnboardingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the onboarding process for a new customer.
        /// </summary>
        /// <remarks>
        /// This endpoint initiates the customer onboarding by creating an onboarding request.
        /// 
        /// **Request Body (StartOnboardingCommand):**
        /// - FirstName: Customer's first name (required, string).
        /// - LastName: Customer's last name (required, string).
        /// - Email: Customer's email address (required, string). Must contain the '@' symbol for valid format.
        /// - PhoneNumber: Customer's phone number (required, string). Must include the '+' symbol for international format (e.g., +2348012345678).
        /// - Country: Numeric code for the customer's country (required, integer).
        ///   - 1: Nigeria
        ///   - 2: Ghana
        ///   - 3: Kenya
        ///   Pass the corresponding number for the country.
        /// - IdentityType: Numeric code for the identity type (required, integer). Must match the selected country code.
        ///   - If Country is 1 (Nigeria), IdentityType should be 1 (BVN).
        ///   - If Country is 2 (Ghana), IdentityType should be 2 (Ghana Card).
        ///   - If Country is 3 (Kenya), IdentityType should be 3 (Kenya National ID).
        /// - IdentityNumber: The customer's identity document number (required, string).
        ///   - For Nigeria (BVN): Must be exactly 11 digits (numeric).
        ///   - For Ghana (Ghana Card): Must be in format GHA-XXXXXXXXX-X (GHA- followed by 9 digits, then - and 1 digit).
        ///   - For Kenya (National ID): Must be between 7 and 9 digits (typically 8 for modern IDs, numeric).
        /// - Other fields may include BVN or additional details based on country.
        /// 
        /// **Validation Notes:**
        /// - Ensure country and identity type match to avoid validation errors.
        /// - Phone number and email formats are strictly validated.
        /// - Invalid formats will result in 400 Bad Request.
        /// 
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/onboarding/start
        /// {
        ///   "firstName": "John",
        ///   "lastName": "Doe",
        ///   "email": "john.doe@example.com",
        ///   "phoneNumber": "+2348012345678",
        ///   "country": 1,
        ///   "identityType": 1,
        ///   "identityNumber": "12345678901"
        /// }
        /// ```
        /// 
        /// **Response:**
        /// - 200 OK: Onboarding started successfully with reference ID.
        /// - 400 Bad Request: Validation errors or onboarding failure.
        /// - 500 Internal Server Error: Unexpected server issues.
        /// </remarks>
        /// <param name="request">The onboarding request details including customer information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the onboarding response with request reference on success, or error details.</returns>
        /// <response code="200">Onboarding started successfully with reference ID</response>
        /// <response code="400">Bad request - validation errors or duplicate request</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpPost("start")]
        [ProducesResponseType(typeof(StartOnboardingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        // CRITICAL: Remove "Description" parameter to let XML <remarks> show through!
        [SwaggerOperation(
            Summary = "Start customer onboarding",
            OperationId = "StartOnboarding",
            Tags = new[] { "Onboarding" }
        )]
        public async Task<IActionResult> StartOnboarding(
            [FromBody] StartOnboardingCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Onboarding Failed",
                    Detail = result.Error,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the status of an existing onboarding request.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the current status of the onboarding process using the provided onboarding ID.
        /// 
        /// **Path Parameter:**
        /// - onboardingId: The unique GUID identifier for the onboarding request (required).
        /// 
        /// **Possible Statuses:**
        /// - Pending: Awaiting processing.
        /// - In Progress: BVN verification or account creation in process.
        /// - Completed: Onboarding successful with account created.
        /// - Failed: Onboarding failed (details in response).
        /// 
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/onboarding/{onboardingId}/status
        /// ```
        /// 
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "onboardingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "status": "Completed",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-15T10:35:00Z"
        /// }
        /// ```
        /// 
        /// **Response:**
        /// - 200 OK: Status details returned.
        /// - 404 Not Found: Onboarding request not found.
        /// - 500 Internal Server Error: Unexpected issues.
        /// </remarks>
        /// <param name="onboardingId">The GUID of the onboarding request to check.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the onboarding status details on success, or error if not found.</returns>
        /// <response code="200">Status retrieved successfully</response>
        /// <response code="404">Onboarding request not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("{onboardingId:guid}/status")]
        [ProducesResponseType(typeof(OnboardingStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        // CRITICAL: Remove "Description" parameter to let XML <remarks> show through!
        [SwaggerOperation(
            Summary = "Get onboarding status",
            OperationId = "GetOnboardingStatus",
            Tags = new[] { "Onboarding" }
        )]
        public async Task<IActionResult> GetOnboardingStatus(
            Guid onboardingId,
            CancellationToken cancellationToken)
        {
            var query = new GetOnboardingStatusQuery { OnboardingId = onboardingId };
            var result = await _mediator.Send(query, cancellationToken);
            if (result.IsFailure)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Onboarding Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }
            return Ok(result.Value);
        }
    }
}