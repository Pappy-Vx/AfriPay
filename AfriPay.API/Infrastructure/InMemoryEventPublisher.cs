using AfriPay.APP.EventHandlers;
using AfriPay.CORE.Common;
using AfriPay.CORE.Events;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure
{
    public class InMemoryEventPublisher : IEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryEventPublisher> _logger;

        public InMemoryEventPublisher(
            IServiceProvider serviceProvider,
            ILogger<InMemoryEventPublisher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            var eventType = @event.GetType().Name;
            _logger.LogInformation("📢 Publishing event: {EventType} (EventId: {EventId})", eventType, @event.EventId);

            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Route to appropriate handler based on event type
                switch (@event)
                {
                    case OnboardingRequestedEvent onboardingEvent:
                        _logger.LogInformation("→ Routing to OnboardingRequestedHandler");
                        var onboardingHandler = scope.ServiceProvider.GetRequiredService<OnboardingRequestedHandler>();
                        await onboardingHandler.HandleAsync(onboardingEvent, cancellationToken);
                        _logger.LogInformation("✅ OnboardingRequestedHandler completed");
                        break;

                    case BvnVerifiedForOnboardingEvent bvnEvent:
                        _logger.LogInformation("→ Routing to BvnVerifiedForOnboardingHandler");
                        var bvnHandler = scope.ServiceProvider.GetRequiredService<BvnVerifiedForOnboardingHandler>();
                        await bvnHandler.HandleAsync(bvnEvent, cancellationToken);
                        _logger.LogInformation("✅ BvnVerifiedForOnboardingHandler completed");
                        break;

                    case CustomerCreatedEvent customerEvent:
                        _logger.LogInformation("→ Routing to CustomerCreatedHandler");
                        var customerHandler = scope.ServiceProvider.GetRequiredService<CustomerCreatedHandler>();
                        await customerHandler.HandleAsync(customerEvent, cancellationToken);
                        _logger.LogInformation("✅ CustomerCreatedHandler completed");
                        break;

                    case OnboardingCompletedEvent completedEvent:
                        _logger.LogInformation("✅ Onboarding completed: {OnboardingId}", completedEvent.OnboardingId);
                        break;

                    default:
                        _logger.LogWarning("⚠️  No handler found for event: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling event: {EventType}", eventType);
                throw;
            }
        }

        public async Task PublishManyAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            var eventsList = events.ToList();
            _logger.LogInformation("📢 Publishing {Count} events", eventsList.Count);

            foreach (var @event in eventsList)
            {
                await PublishAsync(@event, cancellationToken);
            }

            _logger.LogInformation("✅ All {Count} events published", eventsList.Count);
        }
    }

    //public class InMemoryEventPublisher : IEventPublisher
    //{
    //    private readonly IServiceProvider _serviceProvider;
    //    private readonly ILogger<InMemoryEventPublisher> _logger;

    //    public InMemoryEventPublisher(
    //        IServiceProvider serviceProvider,
    //        ILogger<InMemoryEventPublisher> logger)
    //    {
    //        _serviceProvider = serviceProvider;
    //        _logger = logger;
    //    }

    //    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    //        where TEvent : IDomainEvent
    //    {
    //        _logger.LogInformation("Publishing event: {EventType}", @event.GetType().Name);

    //        using var scope = _serviceProvider.CreateScope();

    //        // Route to appropriate handler based on event type
    //        switch (@event)
    //        {
    //            case OnboardingRequestedEvent onboardingEvent:
    //                var onboardingHandler = scope.ServiceProvider.GetRequiredService<OnboardingRequestedHandler>();
    //                await onboardingHandler.HandleAsync(onboardingEvent, cancellationToken);
    //                break;

    //            case BvnVerifiedForOnboardingEvent bvnEvent:
    //                var bvnHandler = scope.ServiceProvider.GetRequiredService<BvnVerifiedForOnboardingHandler>();
    //                await bvnHandler.HandleAsync(bvnEvent, cancellationToken);
    //                break;

    //            case CustomerCreatedEvent customerEvent:
    //                var customerHandler = scope.ServiceProvider.GetRequiredService<CustomerCreatedHandler>();
    //                await customerHandler.HandleAsync(customerEvent, cancellationToken);
    //                break;

    //            default:
    //                _logger.LogWarning("No handler found for event: {EventType}", @event.GetType().Name);
    //                break;
    //        }
    //    }

    //    public async Task PublishManyAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    //    {
    //        foreach (var @event in events)
    //        {
    //            await PublishAsync(@event, cancellationToken);
    //        }
    //    }
    //}
}
