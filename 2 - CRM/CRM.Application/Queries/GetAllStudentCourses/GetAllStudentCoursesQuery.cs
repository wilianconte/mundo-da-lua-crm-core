using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudentCourses;

public record GetAllStudentCoursesQuery : IRequest<Result<IReadOnlyList<StudentCourseDto>>>;
