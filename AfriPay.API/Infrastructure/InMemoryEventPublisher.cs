using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using MediatR;

namespace AfriPay.API.Infrastructure
{
    public class InMemoryEventPublisher : IEventPublisher
    {
        private readonly IPublisher _publisher;
        private readonly ILogger<InMemoryEventPublisher> _logger;

        public InMemoryEventPublisher(
            IPublisher publisher,
            ILogger<InMemoryEventPublisher> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            var eventType = @event.GetType().Name;
            _logger.LogInformation("📢 Publishing event: {EventType} (EventId: {EventId})", eventType, @event.EventId);

            try
            {
                // Use MediatR to publish the event to all registered handlers
                await _publisher.Publish(@event, cancellationToken);
                _logger.LogInformation("✅ Event {EventType} published successfully", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error publishing event: {EventType}", eventType);
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
}
