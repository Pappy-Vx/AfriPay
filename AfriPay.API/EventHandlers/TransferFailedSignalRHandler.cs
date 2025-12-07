using AfriPay.API.Hubs;
using AfriPay.CORE.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AfriPay.API.EventHandlers;

public class TransferFailedSignalRHandler : INotificationHandler<TransferFailedEvent>
{
  private readonly IHubContext<AccountBalanceHub, IAccountBalanceClient> _hubContext;
  private readonly ILogger<TransferFailedSignalRHandler> _logger;

  public TransferFailedSignalRHandler(
      IHubContext<AccountBalanceHub, IAccountBalanceClient> hubContext,
      ILogger<TransferFailedSignalRHandler> logger)
  {
    _hubContext = hubContext;
    _logger = logger;
  }

  public async Task Handle(TransferFailedEvent notification, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Sending SignalR notification for failed transfer: {TransferId}",
        notification.TransferId.Value);

    // Notify sender about failure
    await _hubContext.Clients
        .Group($"customer_{notification.SourceCustomerId.Value}")
        .BalanceUpdated(new BalanceUpdateNotification
        {
          AccountId = Guid.Empty, // No specific account
          Amount = 0,
          Currency = string.Empty,
          Type = "TRANSFER_FAILED",
          Timestamp = DateTime.UtcNow
        });

    _logger.LogInformation("SignalR failure notification sent for transfer: {TransferId}",
        notification.TransferId.Value);
  }
}