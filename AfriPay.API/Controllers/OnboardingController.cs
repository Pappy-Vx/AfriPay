using AfriPay.APP.Onboarding.Commands.StartOnboarding;
using AfriPay.APP.Onboarding.Queries.GetOnboardingStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for managing customer onboarding
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            IMediator mediator,
            ILogger<OnboardingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Start the onboarding process for a new customer
        /// </summary>
        /// <param name="request">Onboarding request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Onboarding response with request reference</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(StartOnboardingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
        /// Get the status of an onboarding request
        /// </summary>
        /// <param name="onboardingId">Onboarding request ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Onboarding status details</returns>
        [HttpGet("{onboardingId:guid}/status")]
        [ProducesResponseType(typeof(OnboardingStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
