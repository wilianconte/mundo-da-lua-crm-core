using FluentValidation;

namespace MyCRM.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId é obrigatório.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token é obrigatório.")
            .MaximumLength(512).WithMessage("Refresh token inválido.");
    }
}
