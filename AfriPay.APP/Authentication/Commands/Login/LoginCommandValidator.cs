using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfriPay.APP.Authentication.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.UserTag)
                .NotEmpty()
                .WithMessage("UserTag is required")
                .MinimumLength(3)
                .WithMessage("Invalid UserTag format")
                .MaximumLength(30)
                .WithMessage("UserTag must not exceed 30 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters");
        }
    }
}
