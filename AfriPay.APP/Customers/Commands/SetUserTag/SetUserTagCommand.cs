using AfriPay.CORE.Common;
using MediatR;

namespace AfriPay.APP.Customers.Commands.SetUserTag;

public record SetUserTagCommand(Guid CustomerId, string UserTag) : IRequest<Result>;