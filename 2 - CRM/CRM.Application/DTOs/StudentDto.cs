namespace MyCRM.CRM.Application.DTOs;

public record StudentDto(
    Guid Id,
    Guid TenantId,
    Guid PersonId,
    Guid? UnitId,
    StudentEnrollmentStatus EnrollmentStatus,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
