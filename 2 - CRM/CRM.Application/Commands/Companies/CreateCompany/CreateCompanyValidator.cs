using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Companies.CreateCompany;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("Legal name is required.")
            .MaximumLength(300);

        RuleFor(x => x.TradeName)
            .MaximumLength(300)
            .When(x => x.TradeName is not null);

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(30)
            .When(x => x.RegistrationNumber is not null);

        RuleFor(x => x.StateRegistration)
            .MaximumLength(30)
            .When(x => x.StateRegistration is not null);

        RuleFor(x => x.MunicipalRegistration)
            .MaximumLength(30)
            .When(x => x.MunicipalRegistration is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(254)
            .When(x => x.Email is not null);

        RuleFor(x => x.PrimaryPhone)
            .MaximumLength(30)
            .When(x => x.PrimaryPhone is not null);

        RuleFor(x => x.SecondaryPhone)
            .MaximumLength(30)
            .When(x => x.SecondaryPhone is not null);

        RuleFor(x => x.WhatsAppNumber)
            .MaximumLength(30)
            .When(x => x.WhatsAppNumber is not null);

        RuleFor(x => x.Website)
            .MaximumLength(500)
            .When(x => x.Website is not null);

        RuleFor(x => x.ContactPersonName)
            .MaximumLength(300)
            .When(x => x.ContactPersonName is not null);

        RuleFor(x => x.ContactPersonEmail)
            .EmailAddress().WithMessage("Contact person email is not valid.")
            .MaximumLength(254)
            .When(x => x.ContactPersonEmail is not null);

        RuleFor(x => x.ContactPersonPhone)
            .MaximumLength(30)
            .When(x => x.ContactPersonPhone is not null);

        RuleFor(x => x.CompanyType)
            .IsInEnum().WithMessage("Invalid company type value.")
            .When(x => x.CompanyType is not null);

        RuleFor(x => x.Industry)
            .MaximumLength(150)
            .When(x => x.Industry is not null);

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(2000)
            .When(x => x.ProfileImageUrl is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
