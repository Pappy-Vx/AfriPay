using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;
using AfriPay.CORE.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.Transfers.Queries.GetTransferById;

public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, Result<TransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTransferByIdQueryHandler> _logger;

    public GetTransferByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTransferByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TransferDto>> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transferId = TransferId.Create(request.TransferId);
        var transfer = await _unitOfWork.Transfers.GetByIdAsync(transferId, cancellationToken);

        if (transfer == null)
        {
            _logger.LogWarning("Transfer not found: {TransferId}", request.TransferId);
            return Result<TransferDto>.Failure("Transfer not found");
        }

        var dto = new TransferDto
        {
            TransferId = transfer.Id.Value,
            TransferReference = transfer.TransferReference,
            IdempotencyKey = transfer.IdempotencyKey,
            SourceAccountId = transfer.SourceAccountId.Value,
            SourceCustomerId = transfer.SourceCustomerId.Value,
            DestinationAccountId = transfer.DestinationAccountId.Value,
            DestinationCustomerId = transfer.DestinationCustomerId.Value,
            DestinationUserTag = transfer.DestinationUserTag,
            Amount = transfer.Amount.Amount,
            Currency = transfer.Amount.Currency,
            Fee = transfer.Fee?.Amount,
            TotalDebitAmount = transfer.TotalDebitAmount.Amount,
            Type = transfer.Type.ToString(),
            Status = transfer.Status.ToString(),
            Description = transfer.Description,
            Narration = transfer.Narration,
            FailureReason = transfer.FailureReason,
            CreatedAt = transfer.CreatedAt,
            CompletedAt = transfer.CompletedAt
        };

        return Result<TransferDto>.Success(dto);
    }
}