using AfriPay.APP.Transfers.Queries.GetTransferById;
using MediatR;

namespace AfriPay.APP.Transfers.Queries.GetTransferHistory;

public record GetTransferHistoryQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 50
) : IRequest<List<TransferDto>>;