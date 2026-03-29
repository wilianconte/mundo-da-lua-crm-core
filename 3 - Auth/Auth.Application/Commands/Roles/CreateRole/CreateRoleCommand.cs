using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description,
    string[]? Permissions
) : IRequest<Result<RoleDto>>;
