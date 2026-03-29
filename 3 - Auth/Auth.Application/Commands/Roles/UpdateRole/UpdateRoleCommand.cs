using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    string[]? Permissions
) : IRequest<Result<RoleDto>>;
