using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.UserRoles.RemoveRoleFromUser;

public record RemoveRoleFromUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<Result>;
