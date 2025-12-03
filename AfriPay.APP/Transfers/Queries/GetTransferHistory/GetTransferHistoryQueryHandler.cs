using AfriPay.APP.Transfers.Queries.GetTransferById;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Transfers.Queries.GetTransferHistory;

public class GetTransferHistoryQueryHandler : IRequestHandler<GetTransferHistoryQuery, List<TransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTransferHistoryQueryHandler> _logger;

    public GetTransferHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTransferHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<TransferDto>> Handle(GetTransferHistoryQuery request, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(request.CustomerId);
        var transfers = await _unitOfWork.Transfers.GetByCustomerIdAsync(
            customerId, request.Page, request.PageSize, cancellationToken);

        var dtos = transfers.Select(t => new TransferDto
        {
            TransferId = t.Id.Value,
            TransferReference = t.TransferReference,
            IdempotencyKey = t.IdempotencyKey,
            SourceAccountId = t.SourceAccountId.Value,
            SourceCustomerId = t.SourceCustomerId.Value,
            DestinationAccountId = t.DestinationAccountId.Value,
            DestinationCustomerId = t.DestinationCustomerId.Value,
            DestinationUserTag = t.DestinationUserTag,
            Amount = t.Amount.Amount,
            Currency = t.Amount.Currency,
            Fee = t.Fee?.Amount,
            TotalDebitAmount = t.TotalDebitAmount.Amount,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Description = t.Description,
            Narration = t.Narration,
            FailureReason = t.FailureReason,
            CreatedAt = t.CreatedAt,
            CompletedAt = t.CompletedAt
        }).ToList();

        _logger.LogInformation("Retrieved {Count} transfers for customer {CustomerId}", dtos.Count, request.CustomerId);

        return dtos;
    }
}