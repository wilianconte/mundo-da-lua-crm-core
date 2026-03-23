using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCourses;

public record GetAllCoursesQuery : IRequest<Result<IReadOnlyList<CourseDto>>>;
