namespace MyCRM.CRM.Application.DTOs;

public record ProfessionalScheduleDto(
    Guid Id,
    Guid TenantId,
    Guid ProfessionalId,
    int DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsAvailable,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
