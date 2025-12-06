using AfriPay.CORE.Entities;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for managing customer accounts. This includes retrieving account balances, transactions, and customer account details.
    /// All endpoints require proper authentication if configured in the API.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for database operations.</param>
        /// <param name="logger">The logger for the controller.</param>
        public AccountController(IUnitOfWork unitOfWork, ILogger<AccountController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves the balance and details for a specific account.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the current balance, reserved balance, available balance, and other details for the given account ID.
        ///
        /// **Path Parameter:**
        /// - accountId: The unique GUID identifier for the account (required).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/account/{accountId}/balance
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "accountNumber": "1234567890",
        ///   "balance": 1000.00,
        ///   "reservedBalance": 100.00,
        ///   "availableBalance": 900.00,
        ///   "currency": "NGN",
        ///   "isPrimary": true,
        ///   "lastUpdated": "2024-01-15T10:35:00Z"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Account details returned successfully.
        /// - 404 Not Found: Account not found.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="accountId">The GUID of the account to retrieve balance for.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the account balance and details on success, or error if not found.</returns>
        /// <response code="200">Account balance retrieved successfully</response>
        /// <response code="404">Account not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("{accountId:guid}/balance")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get account balance and details",
            OperationId = "GetAccountBalance",
            Tags = new[] { "Account" }
        )]
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
        /// Retrieves the transaction history for a specific account.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches a paginated list of transactions for the given account ID.
        ///
        /// **Path Parameter:**
        /// - accountId: The unique GUID identifier for the account (required).
        ///
        /// **Query Parameters:**
        /// - page: The page number to retrieve (default: 1).
        /// - pageSize: The number of transactions per page (default: 50, max: 100).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/account/{accountId}/transactions?page=1&amp;pageSize=50
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "page": 1,
        ///   "pageSize": 50,
        ///   "count": 2,
        ///   "transactions": [
        ///     {
        ///       "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "transactionReference": "REF123",
        ///       "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "direction": "Credit",
        ///       "amount": 500.00,
        ///       "currency": "NGN",
        ///       "balanceBefore": 500.00,
        ///       "balanceAfter": 1000.00,
        ///       "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "narration": "Deposit",
        ///       "createdAt": "2024-01-15T10:30:00Z"
        ///     }
        ///   ]
        /// }
        /// ```
        ///
        /// **Validation Notes:**
        /// - Page must be at least 1.
        /// - PageSize must be between 1 and 100.
        ///
        /// **Response:**
        /// - 200 OK: Transactions returned successfully.
        /// - 404 Not Found: Account not found.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="accountId">The GUID of the account to retrieve transactions for.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The page size (default: 50, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns a paginated list of transactions on success, or error if account not found.</returns>
        /// <response code="200">Transactions retrieved successfully</response>
        /// <response code="404">Account not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("{accountId:guid}/transactions")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get account transactions",
            OperationId = "GetAccountTransactions",
            Tags = new[] { "Account" }
        )]
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
        /// Retrieves all accounts associated with a customer.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches a list of accounts for the given customer ID, including balance and other details.
        ///
        /// **Path Parameter:**
        /// - customerId: The unique GUID identifier for the customer (required).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/account/customer/{customerId}
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "accountCount": 1,
        ///   "accounts": [
        ///     {
        ///       "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "accountNumber": "1234567890",
        ///       "balance": 1000.00,
        ///       "availableBalance": 900.00,
        ///       "isPrimary": true
        ///     }
        ///   ]
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Customer accounts returned successfully.
        /// - 404 Not Found: No accounts found for the customer.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="customerId">The GUID of the customer to retrieve accounts for.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns a list of customer accounts on success, or error if none found.</returns>
        /// <response code="200">Customer accounts retrieved successfully</response>
        /// <response code="404">No accounts found for customer</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get customer's accounts",
            OperationId = "GetCustomerAccounts",
            Tags = new[] { "Account" }
        )]
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
        /// Retrieves details for a specific transaction.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the details of a transaction using the provided transaction ID.
        ///
        /// **Path Parameter:**
        /// - transactionId: The unique GUID identifier for the transaction (required).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/account/transaction/{transactionId}
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "transactionReference": "REF123",
        ///   "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "direction": "Credit",
        ///   "amount": 500.00,
        ///   "currency": "NGN",
        ///   "balanceBefore": 500.00,
        ///   "balanceAfter": 1000.00,
        ///   "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "narration": "Deposit",
        ///   "createdAt": "2024-01-15T10:30:00Z"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Transaction details returned successfully.
        /// - 404 Not Found: Transaction not found.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="transactionId">The GUID of the transaction to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the transaction details on success, or error if not found.</returns>
        /// <response code="200">Transaction details retrieved successfully</response>
        /// <response code="404">Transaction not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("transaction/{transactionId:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get transaction details by ID",
            OperationId = "GetTransactionDetails",
            Tags = new[] { "Account" }
        )]
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
        /// Retrieves the transaction history across all accounts for a customer.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches a paginated list of transactions for all accounts associated with the given customer ID.
        ///
        /// **Path Parameter:**
        /// - customerId: The unique GUID identifier for the customer (required).
        ///
        /// **Query Parameters:**
        /// - page: The page number to retrieve (default: 1).
        /// - pageSize: The number of transactions per page (default: 50, max: 100).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/account/customer/{customerId}/transactions?page=1&amp;pageSize=50
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "page": 1,
        ///   "pageSize": 50,
        ///   "count": 2,
        ///   "transactions": [
        ///     {
        ///       "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "transactionReference": "REF123",
        ///       "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "direction": "Credit",
        ///       "amount": 500.00,
        ///       "currency": "NGN",
        ///       "balanceBefore": 500.00,
        ///       "balanceAfter": 1000.00,
        ///       "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "narration": "Deposit",
        ///       "createdAt": "2024-01-15T10:30:00Z"
        ///     }
        ///   ]
        /// }
        /// ```
        ///
        /// **Validation Notes:**
        /// - Page must be at least 1.
        /// - PageSize must be between 1 and 100.
        ///
        /// **Response:**
        /// - 200 OK: Transactions returned successfully.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="customerId">The GUID of the customer to retrieve transactions for.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The page size (default: 50, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns a paginated list of customer transactions on success.</returns>
        /// <response code="200">Customer transactions retrieved successfully</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("customer/{customerId:guid}/transactions")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get customer's transaction history across all accounts",
            OperationId = "GetCustomerTransactions",
            Tags = new[] { "Account" }
        )]
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

        /// <summary>
        /// Manually credits an account by account number. Intended for testing only.
        /// </summary>
        /// <remarks>
        /// This endpoint looks up an account by its account number and applies a credit of the specified amount.
        /// A transaction record is also created for traceability.
        ///
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/account/manual-credit
        /// {
        ///   "accountNumber": "1234567890",
        ///   "amount": 500.00,
        ///   "currency": "NGN"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Account credited successfully.
        /// - 400 Bad Request: Validation error or business rule failure.
        /// - 404 Not Found: Account not found.
        /// </remarks>
        /// <param name="request">Manual credit request payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPost("manual-credit")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Manually credit an account by account number (testing only)",
            OperationId = "ManualCreditAccount",
            Tags = new[] { "Account" }
        )]
        public async Task<IActionResult> ManualCredit(
            [FromBody] ManualCreditRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return BadRequest(new { error = "AccountNumber is required" });

            if (request.Amount <= 0)
                return BadRequest(new { error = "Amount must be greater than zero" });

            var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(
                request.AccountNumber.Trim(), cancellationToken);

            if (account == null)
                return NotFound(new { error = "Account not found" });

            var accountCurrency = account.Balance.Currency;
            var requestedCurrency = string.IsNullOrWhiteSpace(request.Currency)
                ? accountCurrency
                : request.Currency.Trim().ToUpperInvariant();

            if (!string.Equals(accountCurrency, requestedCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    error = $"Currency mismatch. Account currency is '{accountCurrency}', but '{requestedCurrency}' was provided."
                });
            }

            var amount = new Money(request.Amount, accountCurrency);
            var balanceBefore = account.Balance.Amount;
            var transactionReference = $"MANUAL-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var creditResult = account.Credit(amount, transactionReference, "Manual test credit");
            if (!creditResult.IsSuccess)
                return BadRequest(new { error = creditResult.Error });

            var transaction = Transaction.CreateCredit(
                account.AccountId,
                account.CustomerId,
                amount,
                balanceBefore,
                "Manual test credit");

            await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                accountId = account.AccountId.Value,
                accountNumber = account.AccountNumber.Value,
                transactionId = transaction.Id,
                transactionReference = transaction.TransactionReference,
                creditedAmount = amount.Amount,
                currency = amount.Currency,
                balanceBefore,
                balanceAfter = account.Balance.Amount,
                message = "Account credited successfully (manual test operation)"
            });
        }
    }

    public record ManualCreditRequest(string AccountNumber, decimal Amount, string Currency);
}
