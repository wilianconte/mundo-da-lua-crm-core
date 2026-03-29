using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.UserRoles.AssignRoleToUser;

public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<Result>;
