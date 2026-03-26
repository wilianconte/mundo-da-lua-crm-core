using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Employees.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest<Result>;
