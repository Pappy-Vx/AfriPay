using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUnitOfWork unitOfWork, ILogger<AccountController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get account balance and details
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account balance information</returns>
    [HttpGet("{accountId:guid}/balance")]
    public async Task<IActionResult> GetBalance(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(
            AccountId.Create(accountId), cancellationToken);

        if (account == null)
            return NotFound(new { error = "Account not found" });

        var response = new
        {
            accountId = account.AccountId.Value,
            accountNumber = account.AccountNumber.Value,
            balance = account.Balance,
            reservedBalance = account.ReservedBalance,
            availableBalance = account.GetAvailableBalance(),
            currency = "NGN",
            // status = account.Status.ToString(),
            isPrimary = account.PrimaryAccountInfo,
            lastUpdated = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// Get account transactions
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions</returns>
    [HttpGet("{accountId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Validate account exists
        var account = await _unitOfWork.Accounts.GetByIdAsync(
            AccountId.Create(accountId), cancellationToken);

        if (account == null)
            return NotFound(new { error = "Account not found" });

        // Validate pagination
        if (page < 1)
            page = 1;

        if (pageSize < 1 || pageSize > 100)
            pageSize = 50;

        // Get transactions
        var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(
            AccountId.Create(accountId), page, pageSize, cancellationToken);

        var transactionDtos = transactions.Select(t => new
        {
            transactionId = t.Id,
            transactionReference = t.TransactionReference,
            accountId = t.AccountId.Value,
            customerId = t.CustomerId.Value,
            direction = t.Direction.ToString(),
            amount = t.Amount.Amount,
            currency = t.Amount.Currency,
            balanceBefore = t.BalanceBefore,
            balanceAfter = t.BalanceAfter,
            transferId = t.TransferId?.Value,
            narration = t.Narration,
            createdAt = t.CreatedAt
        }).ToList();

        return Ok(new
        {
            accountId,
            page,
            pageSize,
            count = transactionDtos.Count,
            transactions = transactionDtos
        });
    }

    /// <summary>
    /// Get customer's accounts
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer accounts</returns>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerAccounts(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Accounts.GetByCustomerIdAsync(
            CustomerId.Create(customerId), cancellationToken);

        if (!accounts.Any())
            return NotFound(new { error = "No accounts found for customer" });

        var accountDtos = accounts.Select(a => new
        {
            accountId = a.AccountId.Value,
            accountNumber = a.AccountNumber.Value,
            balance = a.Balance,
            availableBalance = a.GetAvailableBalance(),
            // status = a.Status.ToString(),
            isPrimary = a.PrimaryAccountInfo,

        }).ToList();

        return Ok(new
        {
            customerId,
            accountCount = accountDtos.Count,
            accounts = accountDtos
        });
    }

    /// <summary>
    /// Get transaction details by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction details</returns>
    [HttpGet("transaction/{transactionId:guid}")]
    public async Task<IActionResult> GetTransaction(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId, cancellationToken);

        if (transaction == null)
            return NotFound(new { error = "Transaction not found" });

        var response = new
        {
            transactionId = transaction.Id,
            transactionReference = transaction.TransactionReference,
            accountId = transaction.AccountId.Value,
            customerId = transaction.CustomerId.Value,
            direction = transaction.Direction.ToString(),
            amount = transaction.Amount.Amount,
            currency = transaction.Amount.Currency,
            balanceBefore = transaction.BalanceBefore,
            balanceAfter = transaction.BalanceAfter,
            transferId = transaction.TransferId?.Value,
            narration = transaction.Narration,
            createdAt = transaction.CreatedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Get customer's transaction history across all accounts
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transactions</returns>
    [HttpGet("customer/{customerId:guid}/transactions")]
    public async Task<IActionResult> GetCustomerTransactions(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination
        if (page < 1)
            page = 1;

        if (pageSize < 1 || pageSize > 100)
            pageSize = 50;

        // Get transactions
        var transactions = await _unitOfWork.Transactions.GetByCustomerIdAsync(
            CustomerId.Create(customerId), page, pageSize, cancellationToken);

        var transactionDtos = transactions.Select(t => new
        {
            transactionId = t.Id,
            transactionReference = t.TransactionReference,
            accountId = t.AccountId.Value,
            customerId = t.CustomerId.Value,
            direction = t.Direction.ToString(),
            amount = t.Amount.Amount,
            currency = t.Amount.Currency,
            balanceBefore = t.BalanceBefore,
            balanceAfter = t.BalanceAfter,
            transferId = t.TransferId?.Value,
            narration = t.Narration,
            createdAt = t.CreatedAt
        }).ToList();

        return Ok(new
        {
            customerId,
            page,
            pageSize,
            count = transactionDtos.Count,
            transactions = transactionDtos
        });
    }
}