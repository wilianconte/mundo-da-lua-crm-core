using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Professionals.UpdateProfessionalStatus;

public sealed class UpdateProfessionalStatusValidator : AbstractValidator<UpdateProfessionalStatusCommand>
{
    public UpdateProfessionalStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TargetStatus).IsInEnum().WithMessage("Invalid target status.");
    }
}
