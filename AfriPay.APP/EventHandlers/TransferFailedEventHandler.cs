using AfriPay.CORE.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class TransferFailedEventHandler : INotificationHandler<TransferFailedEvent>
{
    private readonly ILogger<TransferFailedEventHandler> _logger;

    public TransferFailedEventHandler(ILogger<TransferFailedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TransferFailedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Transfer failed. TransferId: {TransferId}, Reference: {Reference}, Reason: {Reason}",
            notification.TransferId.Value,
            notification.TransferReference,
            notification.Reason);

        // Additional failure handling:
        // - Send email/SMS about failure (implement notification service)
        // - Log to monitoring system
        // - Alert support team if critical
        // - SignalR notifications should be sent by a separate handler in the API layer

        return Task.CompletedTask;
    }
}
