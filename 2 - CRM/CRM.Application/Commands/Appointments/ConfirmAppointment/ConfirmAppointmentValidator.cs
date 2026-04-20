using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.ConfirmAppointment;

public sealed class ConfirmAppointmentValidator : AbstractValidator<ConfirmAppointmentCommand>
{
    public ConfirmAppointmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ConfirmedBy).NotEmpty().MaximumLength(100);
    }
}
