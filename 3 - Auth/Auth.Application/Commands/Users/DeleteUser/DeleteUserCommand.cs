using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;
