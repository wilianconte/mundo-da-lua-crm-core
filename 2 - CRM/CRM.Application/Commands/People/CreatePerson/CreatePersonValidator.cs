using FluentValidation;

namespace MyCRM.CRM.Application.Commands.People.CreatePerson;

public sealed class CreatePersonValidator : AbstractValidator<CreatePersonCommand>
{
    public CreatePersonValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(300);

        RuleFor(x => x.PreferredName)
            .MaximumLength(150)
            .When(x => x.PreferredName is not null);

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(20)
            .When(x => x.DocumentNumber is not null);

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

        RuleFor(x => x.Nationality)
            .MaximumLength(100)
            .When(x => x.Nationality is not null);

        RuleFor(x => x.Occupation)
            .MaximumLength(150)
            .When(x => x.Occupation is not null);

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(2000)
            .When(x => x.ProfileImageUrl is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value.")
            .When(x => x.Gender is not null);

        RuleFor(x => x.MaritalStatus)
            .IsInEnum().WithMessage("Invalid marital status value.")
            .When(x => x.MaritalStatus is not null);
    }
}
