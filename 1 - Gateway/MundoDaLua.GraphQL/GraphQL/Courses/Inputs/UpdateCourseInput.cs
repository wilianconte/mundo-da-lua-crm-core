using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Courses.Inputs;

public record UpdateCourseInput(
    string Name,
    CourseType Type,
    string? Code,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ScheduleDescription,
    int? Capacity,
    int? Workload,
    Guid? UnitId,
    string? Notes,
    CourseStatus? Status = null
);
