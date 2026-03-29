using MediatR;
using Microsoft.Extensions.Logging;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGen;
    private readonly ILoginAttemptTracker _tracker;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        ITokenGenerator tokenGen,
        ILoginAttemptTracker tracker,
        ILogger<LoginHandler> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _tokenGen = tokenGen;
        _tracker = tracker;
        _logger = logger;
    }

    public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var lockoutKey = $"login:{request.TenantId}:{request.Email.ToLowerInvariant()}";

        if (_tracker.IsLockedOut(lockoutKey))
        {
            _logger.LogWarning("Login bloqueado por excesso de tentativas. TenantId={TenantId} Email={Email}",
                request.TenantId, request.Email);
            return Result<LoginDto>.Failure("LOGIN_LOCKED_OUT",
                "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente em 15 minutos.");
        }

        var user = await _repo.GetByEmailAsync(request.TenantId, request.Email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            _tracker.RecordFailure(lockoutKey);
            _logger.LogWarning("Tentativa de login inválida. TenantId={TenantId} Email={Email}",
                request.TenantId, request.Email);
            return Result<LoginDto>.Failure("INVALID_CREDENTIALS", "Email ou senha inválidos.");
        }

        if (!user.IsActive)
            return Result<LoginDto>.Failure("USER_INACTIVE", "Usuário inativo.");

        _tracker.ResetFailures(lockoutKey);
        var (token, expiresAt) = _tokenGen.Generate(user);
        return Result<LoginDto>.Success(new LoginDto(token, expiresAt, user.Id, user.Name, user.Email));
    }
}
