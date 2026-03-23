using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record StudentCourseDto(
    Guid Id,
    Guid TenantId,
    Guid StudentId,
    Guid CourseId,
    DateOnly? EnrollmentDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateOnly? CancellationDate,
    DateOnly? CompletionDate,
    StudentCourseStatus Status,
    string? ClassGroup,
    string? Shift,
    string? ScheduleDescription,
    Guid? UnitId,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
