using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Appointments.ResolveAppointmentTask;

public sealed class ResolveAppointmentTaskValidator : AbstractValidator<ResolveAppointmentTaskCommand>
{
    public ResolveAppointmentTaskValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
        RuleFor(x => x.PenaltyAmount).GreaterThan(0).When(x => x.ApplyPenalty)
            .WithMessage("Penalty amount must be greater than zero when applying penalty.");
    }
}
