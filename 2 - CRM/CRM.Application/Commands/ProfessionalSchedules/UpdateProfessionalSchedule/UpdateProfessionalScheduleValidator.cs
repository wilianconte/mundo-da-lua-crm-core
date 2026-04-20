using FluentValidation;

namespace MyCRM.CRM.Application.Commands.ProfessionalSchedules.UpdateProfessionalSchedule;

public sealed class UpdateProfessionalScheduleValidator : AbstractValidator<UpdateProfessionalScheduleCommand>
{
    public UpdateProfessionalScheduleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be greater than StartTime.");
    }
}
