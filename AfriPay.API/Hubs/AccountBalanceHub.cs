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
            _logger.LogInformation("Customer {CustomerId} connected to balance hub", customerId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var customerId = Context.User?.FindFirst("CustomerId")?.Value;
        _logger.LogInformation("Customer {CustomerId} disconnected from balance hub", customerId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to a specific account's updates
    /// </summary>
    public async Task SubscribeToAccount(Guid accountId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to account {AccountId}",
            Context.ConnectionId, accountId);
    }

    /// <summary>
    /// Unsubscribe from a specific account's updates
    /// </summary>
    public async Task UnsubscribeFromAccount(Guid accountId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from account {AccountId}",
            Context.ConnectionId, accountId);
    }
}

public interface IAccountBalanceClient
{
    Task BalanceUpdated(BalanceUpdateNotification notification);
    Task TransferReceived(TransferNotification notification);
    Task TransferFailed(TransferFailedNotification notification);
}

public class BalanceUpdateNotification
{
    public Guid AccountId { get; set; }
    public decimal NewBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // CREDIT, DEBIT, TRANSFER_FAILED
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TransferNotification
{
    public string TransferReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string FromUserTag { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class TransferFailedNotification
{
    public string TransferReference { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}