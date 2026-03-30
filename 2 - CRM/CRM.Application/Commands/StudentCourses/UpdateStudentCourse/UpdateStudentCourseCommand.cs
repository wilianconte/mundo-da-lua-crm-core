using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;

public record UpdateStudentCourseCommand(
    Guid Id,
    DateOnly? EnrollmentDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ClassGroup,
    string? Shift,
    string? ScheduleDescription,
    Guid? UnitId,
    string? Notes,
    StudentCourseStatus? Status
) : IRequest<Result<StudentCourseDto>>;
