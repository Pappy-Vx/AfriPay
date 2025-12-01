using AfriPay.CORE.Enums;
using AfriPay.CORE.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfriPay.DAL.BackgroundJobs
{
    public class OnboardingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnboardingWorker> _logger;

        public OnboardingWorker(
            IServiceProvider serviceProvider,
            ILogger<OnboardingWorker> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Onboarding Worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IOnboardingRequestRepository>();

                    // Get pending requests
                    var pendingRequests = await repository.GetPendingRequestsAsync(stoppingToken);

                    foreach (var request in pendingRequests)
                    {
                        _logger.LogInformation(
                            "Processing onboarding request {RequestReference} with status {Status}",
                            request.RequestReference,
                            request.Status);

                        try
                        {
                            // Process based on current status
                            switch (request.Status)
                            {
                                case OnboardingStatus.Initiated:
                                    _logger.LogInformation(
                                        "Onboarding {RequestReference} initiated - waiting for identity verification",
                                        request.RequestReference);
                                    break;

                                case OnboardingStatus.BvnVerificationPending:
                                    _logger.LogInformation(
                                        "Onboarding {RequestReference} - BVN verification pending",
                                        request.RequestReference);
                                    break;

                                case OnboardingStatus.BvnVerified:
                                    _logger.LogInformation(
                                        "Onboarding {RequestReference} - BVN verified, ready for customer creation",
                                        request.RequestReference);
                                    break;

                                default:
                                    _logger.LogDebug(
                                        "Onboarding {RequestReference} in status {Status}",
                                        request.RequestReference,
                                        request.Status);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Error processing onboarding request {RequestReference}",
                                request.RequestReference);
                        }
                    }

                    // Wait 30 seconds before next poll
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in onboarding worker");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Onboarding Worker stopped");
        }
    }
}
