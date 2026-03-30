using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Students.UpdateStudent;

public sealed class UpdateStudentValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Student Id is required.");

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .When(x => x.RegistrationNumber is not null);

        RuleFor(x => x.SchoolName)
            .MaximumLength(200)
            .When(x => x.SchoolName is not null);

        RuleFor(x => x.GradeOrClass)
            .MaximumLength(100)
            .When(x => x.GradeOrClass is not null);

        RuleFor(x => x.EnrollmentType)
            .MaximumLength(100)
            .When(x => x.EnrollmentType is not null);

        RuleFor(x => x.ClassGroup)
            .MaximumLength(50)
            .When(x => x.ClassGroup is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
