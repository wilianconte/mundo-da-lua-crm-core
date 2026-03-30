using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public sealed class UpdateStudentValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Student Id is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
