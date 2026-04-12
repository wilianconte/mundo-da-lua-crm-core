using MediatR;
using Microsoft.Extensions.Logging;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Auth.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        ILogger<ResetPasswordHandler> logger)
    {
        _repo   = repo;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _repo.GetByPasswordResetTokenAsync(request.Token, ct);
        if (user is null)
            return Result<bool>.Failure("INVALID_RESET_TOKEN", "Token inválido ou já utilizado.");

        if (user.PasswordResetTokenExpiresAt <= DateTime.UtcNow)
            return Result<bool>.Failure("EXPIRED_RESET_TOKEN", "Este link de recuperação expirou. Solicite um novo.");

        if (request.NewPassword != request.NewPasswordConfirmation)
            return Result<bool>.Failure("VALIDATION_ERROR", "As senhas não coincidem.");

        var hash = _hasher.Hash(request.NewPassword);
        user.ResetPassword(hash);
        _repo.Update(user);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Senha redefinida com sucesso para o usuário {UserId}", user.Id);

        return Result<bool>.Success(true);
    }
}
