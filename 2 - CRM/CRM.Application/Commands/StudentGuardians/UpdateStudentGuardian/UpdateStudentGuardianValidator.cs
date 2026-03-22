using FluentValidation;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.UpdateStudentGuardian;

public sealed class UpdateStudentGuardianValidator : AbstractValidator<UpdateStudentGuardianCommand>
{
    public UpdateStudentGuardianValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("StudentGuardian Id is required.");

        RuleFor(x => x.RelationshipType)
            .IsInEnum().WithMessage("Invalid relationship type value.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
