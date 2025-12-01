using AfriPay.CORE.Common;
using AfriPay.APP.Onboarding.Queries.GetOnboardingStatus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Onboarding.Queries.GetOnboardingById
{
    /// <summary>
    /// Handler for GetOnboardingByIdQuery
    /// Delegates to GetOnboardingStatusQueryHandler for implementation
    /// </summary>
    public class GetOnboardingByIdQueryHandler : IRequestHandler<GetOnboardingByIdQuery, Result<OnboardingStatusResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetOnboardingByIdQueryHandler> _logger;

        public GetOnboardingByIdQueryHandler(
            IMediator mediator,
            ILogger<GetOnboardingByIdQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<OnboardingStatusResponse>> Handle(
            GetOnboardingByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Delegating to GetOnboardingStatusQuery for {OnboardingId}",
                request.OnboardingId);

            var query = new GetOnboardingStatusQuery
            {
                OnboardingId = request.OnboardingId
            };

            return await _mediator.Send(query, cancellationToken);
        }
    }
}
