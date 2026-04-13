using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Auth.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<Result<bool>>;
