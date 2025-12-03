using AfriPay.APP.Authentication.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for authentication and authorization operations.
    /// Provides endpoints for customer login and token validation.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthenticationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="mediator">The MediatR mediator for sending commands.</param>
        /// <param name="logger">The logger for the controller.</param>
        public AuthenticationController(
            IMediator mediator,
            ILogger<AuthenticationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticate a customer and receive an access token.
        /// </summary>
        /// <remarks>
        /// This endpoint authenticates a customer using their UserTag and password, returning a JWT access token upon success.
        /// 
        /// **Request Body (LoginCommand):**
        /// - UserTag: Customer's UserTag  (required, string). Must be a valid UserTag format with minimum of 3 letters.
        /// - Password: Customer's password (required, string). Must be at least 8 characters.
        /// 
        /// **Authentication Process:**
        /// 1. Validates UserTag and password format.
        /// 2. Checks if customer exists in the system.
        /// 3. Verifies the customer account is active.
        /// 4. Validates the password against the stored hash.
        /// 5. Generates a JWT token valid for 1 hour (3600 seconds).
        /// 
        /// **JWT Token Claims:**
        /// - sub: Customer ID (GUID)
        /// - UserTag: Customer UserTag
        /// - name: Customer firstname
        /// - customer_id: Customer ID (GUID)
        /// - jti: Unique token identifier
        /// - iat: Token issued at timestamp
        /// 
        /// **Security Notes:**
        /// - Passwords are hashed using PBKDF2 with 100,000 iterations.
        /// - Failed login attempts are logged for security monitoring.
        /// - Generic error messages prevent user enumeration attacks.
        /// - Tokens expire after 1 hour and must be refreshed.
        /// 
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/authentication/login
        /// {
        ///   "UserTag": "John",
        ///   "password": "SecurePassword123!"
        /// }
        /// ```
        /// 
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///     "tokenType": "Bearer",
        ///     "expiresIn": 3600,
        ///     "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "UserTag": "John",
        ///     "Firstname": "John",
        ///     "accountNumber": "1234567890",
        ///     "issuedAt": "2024-01-15T10:30:00Z",
        ///     "expiresAt": "2024-01-15T11:30:00Z"
        ///   }
        /// }
        /// ```
        /// 
        /// **Sample Error Response (401 Unauthorized):**
        /// ```json
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        ///   "title": "Authentication Failed",
        ///   "status": 401,
        ///   "detail": "Invalid UserTag or password",
        ///   "instance": "/api/v1/authentication/login"
        /// }
        /// ```
        /// 
        /// **Common Error Scenarios:**
        /// - 400 Bad Request: Invalid UserTag format or password too short
        /// - 401 Unauthorized: Invalid credentials or inactive account
        /// - 500 Internal Server Error: Database connectivity or unexpected errors
        /// 
        /// **Usage in Subsequent Requests:**
        /// Include the token in the Authorization header:
        /// ```
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        /// ```
        /// </remarks>
        /// <param name="request">The login credentials containing UserTag and password.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Authentication token and customer details on success, or error details on failure.</returns>
        /// <response code="200">Successfully authenticated - returns JWT token and customer details</response>
        /// <response code="400">Bad request - validation errors (invalid UserTag format, password too short)</response>
        /// <response code="401">Unauthorized - invalid credentials or inactive account</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login request received for usertag: {UserTag}", request.UserTag);

            var result = await _mediator.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Login failed for usertag: {UserTag}. Reason: {Error}",
                    request.UserTag,
                    result.Error);

                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication Failed",
                    Detail = result.Error,
                    Status = StatusCodes.Status401Unauthorized,
                    Instance = HttpContext.Request.Path
                });
            }

            _logger.LogInformation(
                "Login successful for customer: {CustomerId}",
                result.Value?.CustomerId);

            return Ok(result.Data);
        }

        /// <summary>
        /// Validate the current authentication token.
        /// </summary>
        /// <remarks>
        /// This endpoint validates if the provided JWT token is still valid and not expired.
        /// 
        /// **Usage:**
        /// Include your JWT token in the Authorization header:
        /// ```
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        /// ```
        /// 
        /// **Validation Checks:**
        /// - Token signature is valid
        /// - Token has not expired
        /// - Token issuer and audience match configuration
        /// - Token has not been tampered with
        /// 
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/authentication/validate
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
        /// ```
        /// 
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "isValid": true,
        ///   "message": "Token is valid",
        ///   "timestamp": "2024-01-15T10:30:00Z"
        /// }
        /// ```
        /// 
        /// **Sample Error Response (401 Unauthorized):**
        /// ```json
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        ///   "title": "Unauthorized",
        ///   "status": 401,
        ///   "detail": "Token is invalid or expired"
        /// }
        /// ```
        /// 
        /// **Note:** This endpoint will be protected with [Authorize] attribute when full authentication is configured.
        /// </remarks>
        /// <returns>Token validation status</returns>
        /// <response code="200">Token is valid and active</response>
        /// <response code="401">Token is invalid, expired, or missing</response>
        [HttpGet("validate")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public IActionResult ValidateToken()
        {
            // This endpoint will be protected by [Authorize] attribute when authentication is configured
            // For now, it's a placeholder for token validation

            return Ok(new
            {
                IsValid = true,
                Message = "Token is valid",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}