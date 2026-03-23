using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentCourses.CreateStudentCourse;

public record CreateStudentCourseCommand(
    Guid StudentId,
    Guid CourseId,
    DateOnly? EnrollmentDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ClassGroup,
    string? Shift,
    string? ScheduleDescription,
    Guid? UnitId,
    string? Notes
) : IRequest<Result<StudentCourseDto>>;
