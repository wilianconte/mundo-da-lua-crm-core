using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.CompleteAppointment;

public sealed class CompleteAppointmentValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
