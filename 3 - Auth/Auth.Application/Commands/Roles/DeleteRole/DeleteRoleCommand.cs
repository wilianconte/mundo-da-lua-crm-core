using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
