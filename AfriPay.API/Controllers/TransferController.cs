using AfriPay.APP.Transfers.Commands.InitiateTransfer;
using AfriPay.APP.Transfers.Queries.GetTransferById;
using AfriPay.APP.Transfers.Queries.GetTransferHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AfriPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransferController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferController> _logger;

    public TransferController(IMediator mediator, ILogger<TransferController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a new transfer between accounts
    /// </summary>
    /// <param name="request">Transfer initiation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer ID and reference</returns>
    [HttpPost]
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
            request.Currency,
            request.Description,
            request.IdempotencyKey
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Transfer initiation failed: {Error}", result.Error);
            return BadRequest(new { error = result.Error });
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
    /// Get transfer details by ID
    /// </summary>
    /// <param name="id">Transfer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransfer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTransferByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get transfer history for a customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of transfers</returns>
    [HttpGet("customer/{customerId:guid}")]
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
    /// Get transfer by reference number
    /// </summary>
    /// <param name="reference">Transfer reference (e.g., TXN-20251203-A1B2C3D4)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer details</returns>
    [HttpGet("reference/{reference}")]
    public async Task<IActionResult> GetTransferByReference(
        string reference,
        CancellationToken cancellationToken)
    {
        // This would require a new query, for now return not implemented
        return StatusCode(501, new { message = "GetByReference not yet implemented" });
    }
}

public record InitiateTransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string? DestinationUserTag,
    decimal Amount,
    string? Currency,
    string? Description,
    string? IdempotencyKey
);