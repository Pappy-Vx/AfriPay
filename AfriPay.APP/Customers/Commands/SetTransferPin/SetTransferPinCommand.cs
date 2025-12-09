using AfriPay.CORE.Common;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AfriPay.APP.Customers.Commands.SetTransferPin;

public record SetTransferPinCommand(
    Guid CustomerId,

    [Required(ErrorMessage = "PIN is required")]
    string Pin,

    [Required(ErrorMessage = "Confirm PIN is required")]
    string ConfirmPin
) : IRequest<Result>;
