using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.DeleteStudent;

public record DeleteStudentCommand(Guid Id) : IRequest<Result>;
