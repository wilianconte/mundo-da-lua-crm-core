using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Auth.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string NewPasswordConfirmation) : IRequest<Result<bool>>;
