using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record CourseDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Code,
    CourseType Type,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ScheduleDescription,
    int? Capacity,
    int? Workload,
    Guid? UnitId,
    CourseStatus Status,
    bool IsActive,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
