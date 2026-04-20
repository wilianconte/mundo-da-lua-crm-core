using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.CancelAppointment;

public sealed class CancelAppointmentValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MinimumNoticeHours).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(1000).When(x => x.Reason is not null);
    }
}
