using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.CreateUser;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    Guid? PersonId,
    bool IsAdmin = false,
    IReadOnlyList<Guid>? RoleIds = null
) : IRequest<Result<UserDto>>;
