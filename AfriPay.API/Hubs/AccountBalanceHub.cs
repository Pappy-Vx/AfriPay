using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AfriPay.API.Hubs;

[Authorize]
public class AccountBalanceHub : Hub<IAccountBalanceClient>
{
  private readonly ILogger<AccountBalanceHub> _logger;

  public AccountBalanceHub(ILogger<AccountBalanceHub> logger)
  {
    _logger = logger;
  }

  public override async Task OnConnectedAsync()
  {
    var customerId = Context.User?.FindFirst("CustomerId")?.Value;

    if (!string.IsNullOrEmpty(customerId))
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{customerId}");
      _logger.LogInformation("Customer {CustomerId} connected to hub", customerId);
    }

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    var customerId = Context.User?.FindFirst("CustomerId")?.Value;
    _logger.LogInformation("Customer {CustomerId} disconnected from hub", customerId);
    await base.OnDisconnectedAsync(exception);
  }
}

public interface IAccountBalanceClient
{
  Task BalanceUpdated(BalanceUpdateNotification notification);
  Task TransferReceived(TransferNotification notification);
}

public class BalanceUpdateNotification
{
  public Guid AccountId { get; set; }
  public decimal NewBalance { get; set; }
  public string Currency { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty; // CREDIT or DEBIT
  public decimal Amount { get; set; }
  public DateTime Timestamp { get; set; }
}

public class TransferNotification
{
  public string TransferReference { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public string FromUserTag { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; }
}