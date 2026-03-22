using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid Id) : IRequest<Result<StudentDto>>;
