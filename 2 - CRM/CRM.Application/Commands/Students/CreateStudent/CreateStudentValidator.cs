using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Students.CreateStudent;

public sealed class CreateStudentValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("PersonId is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
