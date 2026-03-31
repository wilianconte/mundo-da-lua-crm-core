using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    Guid? PersonId,
    bool IsActive,
    string? Password,
    IReadOnlyList<Guid>? RoleIds = null
) : IRequest<Result<UserDto>>;
