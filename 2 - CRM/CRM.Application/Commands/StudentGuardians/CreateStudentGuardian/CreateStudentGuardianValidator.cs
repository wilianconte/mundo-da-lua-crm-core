using FluentValidation;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.CreateStudentGuardian;

public sealed class CreateStudentGuardianValidator : AbstractValidator<CreateStudentGuardianCommand>
{
    public CreateStudentGuardianValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.GuardianPersonId)
            .NotEmpty().WithMessage("GuardianPersonId is required.");

        RuleFor(x => x.RelationshipType)
            .IsInEnum().WithMessage("Invalid relationship type value.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
