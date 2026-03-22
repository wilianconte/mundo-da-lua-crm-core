using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudentGuardians;

public record GetAllStudentGuardiansQuery : IRequest<Result<IReadOnlyList<StudentGuardianDto>>>;
