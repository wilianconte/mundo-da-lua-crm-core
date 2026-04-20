using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewStartDateTime).NotEmpty();
        RuleFor(x => x.OverridePrice).GreaterThan(0).When(x => x.OverridePrice.HasValue)
            .WithMessage("OverridePrice must be greater than zero.");
    }
}
