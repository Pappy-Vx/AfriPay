using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.DAL.Services
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(
            IMediator mediator,
            ILogger<DomainEventDispatcher> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Dispatching domain event: {EventType} - {EventId}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);

                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogInformation(
                    "Successfully dispatched domain event: {EventType}",
                    domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error dispatching domain event: {EventType}",
                    domainEvent.GetType().Name);
                throw;
            }
        }
    }
}
