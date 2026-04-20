using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Inputs;

public record AddressInput(
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "BR");

public record RecurrenceInput(
    RecurrenceFrequency Frequency,
    DateOnly? EndDate,
    int? MaxOccurrences);

public record CreateAppointmentInput(
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
    RecurrenceInput? Recurrence);

public record ConfirmAppointmentInput(
    Guid Id,
    string ConfirmedBy);

public record CancelAppointmentInput(
    Guid Id,
    string? Reason);

public record RescheduleAppointmentInput(
    Guid Id,
    DateTime NewStartDateTime,
    decimal? OverridePrice);

public record CompleteAppointmentInput(
    Guid Id,
    Guid CategoryId);

public record MarkNoShowInput(Guid Id);

public record ResolveAppointmentTaskInput(
    Guid TaskId,
    bool ApplyPenalty,
    decimal PenaltyAmount,
    Guid CategoryId,
    Guid PaymentMethodId);
