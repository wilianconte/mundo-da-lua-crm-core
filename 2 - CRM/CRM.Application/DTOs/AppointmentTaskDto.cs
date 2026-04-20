using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record AppointmentTaskDto(
    Guid Id,
    Guid TenantId,
    Guid AppointmentId,
    AppointmentTaskType Type,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    AppointmentTaskStatus Status,
    string? Result,
    DateTime? ResolvedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
