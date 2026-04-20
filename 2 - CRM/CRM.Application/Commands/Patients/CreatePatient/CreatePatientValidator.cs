using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Patients.CreatePatient;

public sealed class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("PersonId is required.");
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}
