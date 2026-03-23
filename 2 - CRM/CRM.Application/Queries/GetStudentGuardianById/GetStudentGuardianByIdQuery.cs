using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetStudentGuardianById;

public record GetStudentGuardianByIdQuery(Guid Id) : IRequest<Result<StudentGuardianDto>>;
