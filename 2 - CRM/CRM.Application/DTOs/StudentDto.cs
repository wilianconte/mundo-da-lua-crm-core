using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record StudentDto(
    Guid Id,
    Guid TenantId,
    Guid PersonId,
    Guid? UnitId,
    StudentStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
