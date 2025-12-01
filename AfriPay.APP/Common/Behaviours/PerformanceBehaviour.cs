using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AfriPay.APP.Common.Behaviours
{
    /// <summary>
    /// Pipeline behavior for monitoring performance of requests
    /// </summary>
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
        private readonly Stopwatch _timer;

        public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            // Log warning if request takes longer than 500ms
            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds)",
                    requestName,
                    elapsedMilliseconds);
            }

            return response;
        }
    }
}
