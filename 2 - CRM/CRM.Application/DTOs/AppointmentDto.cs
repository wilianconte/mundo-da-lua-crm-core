using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid TenantId,
    Guid ProfessionalId,
    Guid PatientId,
    Guid ServiceId,
    DateTime StartDateTime,
    DateTime EndDateTime,
    AppointmentType Type,
    decimal Price,
    AddressDto? Address,
    string? MeetingLink,
    PaymentReceiverType PaymentReceiver,
    Guid PaymentMethodId,
    AppointmentStatus Status,
    Guid? RecurrenceId,
    string? ConfirmedBy,
    DateTime? ConfirmedAt,
    string? CancellationReason,
    DateTime? CancelledAt,
    bool CancelledWithLateNotice,
    DateTime? NoShowAt,
    DateTime? CompletedAt,
    Guid? RescheduledFrom,
    DateTime? RescheduledAt,
    Guid? TransactionId,
    string? Notes,
    List<string> Warnings,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
