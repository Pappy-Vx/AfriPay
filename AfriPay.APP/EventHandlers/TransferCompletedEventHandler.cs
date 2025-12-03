using AfriPay.CORE.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class TransferCompletedEventHandler : INotificationHandler<TransferCompletedEvent>
{
    private readonly ILogger<TransferCompletedEventHandler> _logger;

    public TransferCompletedEventHandler(ILogger<TransferCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TransferCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transfer completed successfully. TransferId: {TransferId}, Reference: {Reference}, Amount: {Amount}",
            notification.TransferId.Value,
            notification.TransferReference,
            notification.Amount);

        // Additional actions can be added here:
        // - Send email/SMS notifications
        // - Log to analytics
        // - Update reporting dashboards
        // - Trigger webhooks

        return Task.CompletedTask;
    }
}
