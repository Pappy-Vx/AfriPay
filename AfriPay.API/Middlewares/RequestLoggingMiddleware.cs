using System.Diagnostics;

namespace AfriPay.API.Middlewares
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            // Add request ID to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Request-ID"] = requestId;
                return Task.CompletedTask;
            });

            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} started. RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);

            try
            {
                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms. RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "HTTP {RequestMethod} {RequestPath} failed with exception after {ElapsedMilliseconds}ms. RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    requestId);

                throw;
            }
        }
    }
}
