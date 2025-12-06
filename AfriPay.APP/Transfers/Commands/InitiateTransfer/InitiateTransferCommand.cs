using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Transfers.Commands.InitiateTransfer;

public record InitiateTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string? DestinationUserTag,
    decimal Amount,
    string? Currency,
    string? Description,
    string Password,
    string? IdempotencyKey
) : IRequest<Result<Guid>>;
