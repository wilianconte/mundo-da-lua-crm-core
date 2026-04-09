using FluentValidation;

namespace MyCRM.Auth.Application.Commands.Tenants.RegisterTenant;

public sealed class RegisterTenantValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantValidator()
    {
        RuleFor(x => x.CompanyLegalName)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.CompanyCnpj)
            .MaximumLength(18)
            .When(x => x.CompanyCnpj is not null);

        RuleFor(x => x.CompanyEmail)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => x.CompanyEmail is not null);

        RuleFor(x => x.AdminName)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(x => x.AdminCpf)
            .MaximumLength(14)
            .When(x => x.AdminCpf is not null);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(x => x.PasswordConfirmation)
            .NotEmpty()
            .Equal(x => x.Password)
            .WithMessage("Password confirmation does not match.");
    }
}
