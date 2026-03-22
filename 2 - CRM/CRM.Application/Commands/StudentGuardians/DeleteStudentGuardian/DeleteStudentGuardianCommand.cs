using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.StudentGuardians.DeleteStudentGuardian;

public record DeleteStudentGuardianCommand(Guid Id) : IRequest<Result>;
