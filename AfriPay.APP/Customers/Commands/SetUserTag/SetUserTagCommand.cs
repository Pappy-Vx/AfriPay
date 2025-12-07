using AfriPay.CORE.Common;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AfriPay.APP.Customers.Commands.SetUserTag;

public record SetUserTagCommand(
    Guid CustomerId,
    string UserTag,

    [Required(ErrorMessage = "Password is required")]
    string Password,

    [Required(ErrorMessage = "Confirm password is required")]
    string ConfirmPassword
) : IRequest<Result>;