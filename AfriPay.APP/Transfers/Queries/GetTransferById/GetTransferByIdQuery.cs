using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Transfers.Queries.GetTransferById;

public record GetTransferByIdQuery(Guid TransferId) : IRequest<Result<TransferDto>>;