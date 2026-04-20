using FluentValidation;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.Commands.Appointments.CreateAppointment;

public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
        RuleFor(x => x.StartDateTime).NotEmpty();
        RuleFor(x => x.OverridePrice).GreaterThan(0).When(x => x.OverridePrice.HasValue)
            .WithMessage("OverridePrice must be greater than zero (RN-070).");
        RuleFor(x => x.MeetingLink).MaximumLength(500).When(x => x.MeetingLink is not null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
        When(x => x.Recurrence is not null, () =>
        {
            RuleFor(x => x.Recurrence!.Frequency).IsInEnum();
            RuleFor(x => x.Recurrence!.EndDate).Null().When(x => x.Recurrence!.MaxOccurrences is null)
                .WithMessage("Either EndDate or MaxOccurrences is required for recurrence (RN-057).");
        });
        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Address!.Neighborhood).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Address!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Address!.State).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Address!.ZipCode).NotEmpty().MaximumLength(20);
        });
    }
}
