using FluentValidation;

namespace MyCRM.Auth.Application.Commands.LoginByEmail;

public sealed class LoginByEmailValidator : AbstractValidator<LoginByEmailCommand>
{
    public LoginByEmailValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
