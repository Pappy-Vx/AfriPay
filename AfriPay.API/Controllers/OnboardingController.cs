using AfriPay.APP.DTOs;
using AfriPay.APP.Services;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            IOnboardingService onboardingService,
            ILogger<OnboardingController> logger)
        {
            _onboardingService = onboardingService ?? throw new ArgumentNullException(nameof(onboardingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Start the onboarding process for a new customer
        /// </summary>
        /// <param name="request">Onboarding request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Onboarding response with request reference</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(OnboardingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartOnboarding(
            [FromBody] OnboardingStartRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _onboardingService.StartOnboardingAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Onboarding Failed",
                        Detail = result.Error,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing onboarding request");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Get the status of an onboarding request
        /// </summary>
        /// <param name="onboardingId">Onboarding request ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Onboarding status details</returns>
        [HttpGet("{onboardingId:guid}/status")]
        [ProducesResponseType(typeof(OnboardingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOnboardingStatus(
            Guid onboardingId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _onboardingService.GetOnboardingStatusAsync(onboardingId, cancellationToken);

                if (result.IsFailure)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Onboarding Not Found",
                        Detail = result.Error,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving onboarding status for {OnboardingId}", onboardingId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

}
