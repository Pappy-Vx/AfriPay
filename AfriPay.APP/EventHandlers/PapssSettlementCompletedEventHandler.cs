using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public sealed class PapssSettlementCompletedEventHandler
    : INotificationHandler<PapssSettlementCompletedNotification>
{
    private readonly ILogger<PapssSettlementCompletedEventHandler> _logger;

    public PapssSettlementCompletedEventHandler(ILogger<PapssSettlementCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PapssSettlementCompletedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "PAPSS settlement completed for Transfer {TransferId}: {SourceAmount} {SourceCurrency} -> {DestinationAmount} {DestinationCurrency} @ FX {FxRate}",
            notification.TransferId,
            notification.SourceAmount,
            notification.SourceCurrency,
            notification.DestinationAmount,
            notification.DestinationCurrency,
            notification.FxRate);

        // Future extension points:
        // - audit log entry for cross-border transfers
        // - notify external systems (webhooks, reporting, etc.)

        return Task.CompletedTask;
    }
}
