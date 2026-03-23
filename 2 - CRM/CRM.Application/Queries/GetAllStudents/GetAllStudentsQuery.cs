using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<Result<IReadOnlyList<StudentDto>>>;
