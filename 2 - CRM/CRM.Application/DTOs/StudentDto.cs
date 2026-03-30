using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record StudentDto(
    Guid Id,
    Guid TenantId,
    Guid PersonId,
    string? RegistrationNumber,
    string? SchoolName,
    string? GradeOrClass,
    string? EnrollmentType,
    Guid? UnitId,
    string? ClassGroup,
    DateOnly? StartDate,
    StudentStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
