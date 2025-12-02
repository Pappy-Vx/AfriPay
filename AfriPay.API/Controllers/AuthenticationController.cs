using AfriPay.APP.Authentication.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for authentication and authorization operations
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            IMediator mediator,
            ILogger<AuthenticationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticate a customer and receive an access token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication token and customer details</returns>
        /// <response code="200">Successfully authenticated</response>
        /// <response code="400">Invalid credentials or validation error</response>
        /// <response code="401">Authentication failed</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login request received for email: {Email}", request.Email);

            var result = await _mediator.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Login failed for email: {Email}. Reason: {Error}",
                    request.Email,
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
                result.Value.CustomerId);

            return Ok(result.Value);
        }

        /// <summary>
        /// Validate the current authentication token
        /// </summary>
        /// <returns>Token validation status</returns>
        /// <response code="200">Token is valid</response>
        /// <response code="401">Token is invalid or expired</response>
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