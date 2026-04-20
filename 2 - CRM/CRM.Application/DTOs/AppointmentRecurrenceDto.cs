using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record AppointmentRecurrenceDto(
    Guid Id,
    Guid TenantId,
    Guid ParentAppointmentId,
    RecurrenceFrequency Frequency,
    DateOnly? EndDate,
    int? MaxOccurrences,
    int CreatedOccurrences,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
