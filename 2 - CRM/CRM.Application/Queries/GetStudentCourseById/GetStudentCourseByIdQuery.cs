using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentCourseById;

public record GetStudentCourseByIdQuery(Guid Id) : IRequest<Result<StudentCourseDto>>;
