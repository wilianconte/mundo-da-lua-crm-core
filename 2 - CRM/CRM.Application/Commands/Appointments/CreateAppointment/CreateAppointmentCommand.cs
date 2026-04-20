using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid ProfessionalId,
    Guid PatientId,
    Guid ServiceId,
    DateTime StartDateTime,
    AppointmentType Type,
    decimal? OverridePrice,
    PaymentReceiverType PaymentReceiver,
    Guid PaymentMethodId,
    AddressInput? Address,
    string? MeetingLink,
    string? Notes,
    RecurrenceInput? Recurrence
) : IRequest<Result<AppointmentDto>>;

public sealed record AddressInput(
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "BR"
);

public sealed record RecurrenceInput(
    RecurrenceFrequency Frequency,
    DateOnly? EndDate,
    int? MaxOccurrences
);
