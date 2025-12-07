using AfriPay.API.Hubs;
using AfriPay.CORE.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AfriPay.API.EventHandlers;

public class TransferCompletedSignalRHandler : INotificationHandler<TransferCompletedEvent>
{
  private readonly IHubContext<AccountBalanceHub, IAccountBalanceClient> _hubContext;
  private readonly ILogger<TransferCompletedSignalRHandler> _logger;

  public TransferCompletedSignalRHandler(
      IHubContext<AccountBalanceHub, IAccountBalanceClient> hubContext,
      ILogger<TransferCompletedSignalRHandler> logger)
  {
    _hubContext = hubContext;
    _logger = logger;
  }

  public async Task Handle(TransferCompletedEvent notification, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Sending SignalR notification for completed transfer: {TransferId}",
        notification.TransferId.Value);

    // Notify sender (debit)
    await _hubContext.Clients
        .Group($"customer_{notification.SourceCustomerId.Value}")
        .BalanceUpdated(new BalanceUpdateNotification
        {
          AccountId = notification.SourceAccountId.Value,
          Amount = notification.TotalDebitAmount,
          Currency = notification.Currency,
          Type = "DEBIT",
          Timestamp = DateTime.UtcNow
        });

    // Notify receiver (credit)
    await _hubContext.Clients
        .Group($"customer_{notification.DestinationCustomerId.Value}")
        .TransferReceived(new TransferNotification
        {
          TransferReference = notification.TransferReference,
          Amount = notification.Amount,
          FromUserTag = "", // Could be enhanced to include sender info
          Timestamp = DateTime.UtcNow
        });

    await _hubContext.Clients
        .Group($"customer_{notification.DestinationCustomerId.Value}")
        .BalanceUpdated(new BalanceUpdateNotification
        {
          AccountId = notification.DestinationAccountId.Value,
          Amount = notification.Amount,
          Currency = notification.Currency,
          Type = "CREDIT",
          Timestamp = DateTime.UtcNow
        });

    _logger.LogInformation("SignalR notifications sent for transfer: {TransferId}",
        notification.TransferId.Value);
  }
}