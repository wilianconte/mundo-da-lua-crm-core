using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid Id) : IRequest<Result<CourseDto>>;
