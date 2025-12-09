using AfriPay.APP.Transfers.Commands.InitiateTransfer;
using AfriPay.APP.Transfers.Queries.GetTransferById;
using AfriPay.APP.Transfers.Queries.GetTransferHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AfriPay.API.Controllers
{
    /// <summary>
    /// Controller for managing transfers between accounts. This includes initiating transfers and retrieving transfer details and history.
    /// All endpoints require proper authentication if configured in the API.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class TransferController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransferController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferController"/> class.
        /// </summary>
        /// <param name="mediator">The MediatR mediator for sending commands and queries.</param>
        /// <param name="logger">The logger for the controller.</param>
        public TransferController(IMediator mediator, ILogger<TransferController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates a new transfer between accounts.
        /// </summary>
        /// <remarks>
        /// This endpoint starts a transfer process from the source account to the destination account or user tag.
        ///
        /// **FX Transfer Rates:**
        /// The following foreign exchange (FX) rates apply for transfers between different countries/currencies. These rates are used for conversions when source and destination currencies differ.
        /// - **Nigeria ↔ Ghana:**
        ///   - **NGN to GHS: 0.0076** (1 NGN → 0.0076 GHS)
        ///   - **GHS to NGN: 127.14** (1 GHS → 127.14 NGN)
        /// - **Nigeria ↔ Kenya:**
        ///   - **NGN to KES: 0.0896** (1 NGN → 0.0896 KES)
        ///   - **KES to NGN: 11.19** (1 KES → 11.19 NGN)
        /// - **Ghana ↔ Kenya:**
        ///   - **GHS to KES: 10.72** (1 GHS → 10.72 KES)
        ///   - **KES to GHS: 0.093** (1 KES → 0.093 GHS)
        ///
        /// **Request Body (InitiateTransferRequest):**
        /// - SourceAccountId: The GUID of the source account (required).
        /// - DestinationAccountId: The GUID of the destination account (optional if DestinationUserTag is provided).
        /// - DestinationUserTag: The user tag of the destination (optional if DestinationAccountId is provided, e.g., "Kwame").
        /// - Amount: The transfer amount (required, decimal, must be positive).
        /// - Description: A description or narration for the transfer (optional, string).
        /// - Pin: The customer's transfer PIN, required for additional security.
        /// - IdempotencyKey: A unique key to prevent duplicate transfers (optional, string).
        ///
        /// **Validation Notes:**
        /// - Password must be valid for the source customer; otherwise the transfer is rejected.
        /// - Either DestinationAccountId or DestinationUserTag must be provided, but not both.
        /// - Amount must be greater than 0.
        /// - Source and destination must be valid and have sufficient balance.
        /// - Invalid requests will result in 400 Bad Request.
        ///
        /// **Sample Request:**
        /// ```json
        /// POST /api/v1/transfer
        /// {
        ///   "sourceAccountId": "FA9C2912-F46F-457A-961E-B526FE7AFB72",
        ///   "destinationAccountId": "B11EDBE9-8D80-4D46-A27B-05230D410E40",
        ///   "destinationUserTag": "Kwame",
        ///   "amount": 8000,
        ///   "description": "paid kwame fees",
        ///   "pin": "1234",
        ///   "idempotencyKey": "key-3"
        /// }
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "message": "Transfer initiated successfully",
        ///   "transfer": {
        ///     "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "reference": "TXN-20251203-A1B2C3D4",
        ///     "status": "Completed",
        ///     "amount": 500.00,
        ///     "currency": "NGN",
        ///     "createdAt": "2025-12-04T10:30:00Z"
        ///   }
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Transfer initiated successfully with details.
        /// - 400 Bad Request: Validation errors or insufficient balance.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="request">The transfer initiation request details.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the transfer response with details on success, or error details.</returns>
        /// <response code="200">Transfer initiated successfully</response>
        /// <response code="400">Bad request - validation errors or transfer failure</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpPost]
        [EnableRateLimiting("TransfersPolicy")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Initiate a new transfer between accounts",
            OperationId = "InitiateTransfer",
            Tags = new[] { "Transfer" }
        )]
        public async Task<IActionResult> InitiateTransfer(
            [FromBody] InitiateTransferRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transfer request received from account {SourceAccountId} to {DestinationAccountId}, Amount: {Amount}",
                request.SourceAccountId, request.DestinationAccountId, request.Amount);
            var command = new InitiateTransferCommand(
                request.SourceAccountId,
                request.DestinationAccountId,
                request.DestinationUserTag,
                request.Amount,
                request.Description,
                request.Pin,
                request.IdempotencyKey
            );
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Transfer initiation failed: {Error}", result.Error);
                return BadRequest(new ProblemDetails
                {
                    Title = "Transfer Failed",
                    Detail = result.Error,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }
            // Get the full transfer details to return
            var transferQuery = new GetTransferByIdQuery(result.Value);
            var transferResult = await _mediator.Send(transferQuery, cancellationToken);
            if (transferResult.IsSuccess)
            {
                return Ok(new
                {
                    message = "Transfer initiated successfully",
                    transfer = transferResult.Value
                });
            }
            // Fallback if we can't fetch the transfer details
            return Ok(new
            {
                message = "Transfer initiated successfully",
                transferId = result.Value
            });
        }

        /// <summary>
        /// Retrieves the details of a specific transfer by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the details of a transfer using the provided transfer ID.
        ///
        /// **Path Parameter:**
        /// - id: The unique GUID identifier for the transfer (required).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/transfer/{id}
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "reference": "TXN-20251203-A1B2C3D4",
        ///   "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "destinationAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        ///   "amount": 500.00,
        ///   "currency": "NGN",
        ///   "status": "Completed",
        ///   "createdAt": "2025-12-04T10:30:00Z"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Transfer details returned.
        /// - 404 Not Found: Transfer not found.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="id">The GUID of the transfer to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the transfer details on success, or error if not found.</returns>
        /// <response code="200">Transfer details retrieved successfully</response>
        /// <response code="404">Transfer not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get transfer details by ID",
            OperationId = "GetTransferById",
            Tags = new[] { "Transfer" }
        )]
        public async Task<IActionResult> GetTransfer(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetTransferByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new ProblemDetails
                {
                    Title = "Transfer Not Found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the transfer history for a specific customer.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches a paginated list of transfers associated with the given customer ID.
        ///
        /// **Path Parameter:**
        /// - customerId: The unique GUID identifier for the customer (required).
        ///
        /// **Query Parameters:**
        /// - page: The page number to retrieve (default: 1).
        /// - pageSize: The number of transfers per page (default: 50, max: 100).
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/transfer/customer/{customerId}?page=1&amp;pageSize=50
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "page": 1,
        ///   "pageSize": 50,
        ///   "count": 2,
        ///   "transfers": [
        ///     {
        ///       "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "reference": "TXN-20251203-A1B2C3D4",
        ///       "amount": 500.00,
        ///       "currency": "NGN",
        ///       "status": "Completed",
        ///       "createdAt": "2025-12-04T10:30:00Z"
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
        /// - 200 OK: Transfer history returned successfully.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// </remarks>
        /// <param name="customerId">The GUID of the customer to retrieve transfer history for.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The page size (default: 50, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns a paginated list of transfers.</returns>
        /// <response code="200">Transfer history retrieved successfully</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get transfer history for a customer",
            OperationId = "GetTransferHistory",
            Tags = new[] { "Transfer" }
        )]
        public async Task<IActionResult> GetTransferHistory(
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
            var query = new GetTransferHistoryQuery(customerId, page, pageSize);
            var transfers = await _mediator.Send(query, cancellationToken);
            return Ok(new
            {
                page,
                pageSize,
                count = transfers.Count,
                transfers
            });
        }

        /// <summary>
        /// Retrieves the details of a transfer by reference number.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches the details of a transfer using the provided reference number.
        ///
        /// **Path Parameter:**
        /// - reference: The unique reference string for the transfer (required, e.g., "TXN-20251203-A1B2C3D4").
        ///
        /// **Sample Request:**
        /// ```
        /// GET /api/v1/transfer/reference/{reference}
        /// ```
        ///
        /// **Sample Success Response (200 OK):**
        /// ```json
        /// {
        ///   "transferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "reference": "TXN-20251203-A1B2C3D4",
        ///   "sourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "destinationAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        ///   "amount": 500.00,
        ///   "currency": "NGN",
        ///   "status": "Completed",
        ///   "createdAt": "2025-12-04T10:30:00Z"
        /// }
        /// ```
        ///
        /// **Response:**
        /// - 200 OK: Transfer details returned.
        /// - 404 Not Found: Transfer not found.
        /// - 500 Internal Server Error: Unexpected server issue.
        /// - 501 Not Implemented: Feature not yet available.
        /// </remarks>
        /// <param name="reference">The reference string of the transfer to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Returns the transfer details on success, or error if not found.</returns>
        /// <response code="200">Transfer details retrieved successfully</response>
        /// <response code="404">Transfer not found</response>
        /// <response code="500">Internal server error - unexpected system error</response>
        /// <response code="501">Not implemented</response>
        [HttpGet("reference/{reference}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status501NotImplemented)]
        [SwaggerOperation(
            Summary = "Get transfer by reference number",
            OperationId = "GetTransferByReference",
            Tags = new[] { "Transfer" }
        )]
        public async Task<IActionResult> GetTransferByReference(
            string reference,
            CancellationToken cancellationToken)
        {
            // This would require a new query, for now return not implemented
            return StatusCode(501, new ProblemDetails
            {
                Title = "Not Implemented",
                Detail = "GetByReference not yet implemented",
                Status = StatusCodes.Status501NotImplemented,
                Instance = HttpContext.Request.Path
            });
        }
    }

    public class InitiateTransferRequest
    {
        public Guid SourceAccountId { get; set; }
        public Guid DestinationAccountId { get; set; }
        public string? DestinationUserTag { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "PIN is required")]
        [MinLength(4, ErrorMessage = "PIN must be at least 4 characters")]
        [MaxLength(12, ErrorMessage = "PIN cannot exceed 12 characters")]
        [SwaggerSchema(Description = "Customer transfer PIN used to authorize the transfer.", Format = "password")]
        public string Pin { get; set; } = string.Empty;
        public string? IdempotencyKey { get; set; }
    }
}