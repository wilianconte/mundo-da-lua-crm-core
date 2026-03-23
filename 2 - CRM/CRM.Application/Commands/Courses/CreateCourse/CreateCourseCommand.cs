using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Courses.CreateCourse;

public record CreateCourseCommand(
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
    string? Notes
) : IRequest<Result<CourseDto>>;
