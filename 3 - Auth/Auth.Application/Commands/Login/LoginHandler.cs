using MediatR;
using Microsoft.Extensions.Logging;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using DomainRefreshToken = MyCRM.Auth.Domain.Entities.RefreshToken;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginDto>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGen;
    private readonly IRefreshTokenGenerator _refreshGen;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly ILoginAttemptTracker _tracker;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        ITokenGenerator tokenGen,
        IRefreshTokenGenerator refreshGen,
        IRefreshTokenRepository refreshRepo,
        ILoginAttemptTracker tracker,
        ILogger<LoginHandler> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _tokenGen = tokenGen;
        _refreshGen = refreshGen;
        _refreshRepo = refreshRepo;
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

        var (accessToken, accessExpiresAt) = _tokenGen.Generate(user);
        var (rawRefreshToken, tokenHash, refreshExpiresAt) = _refreshGen.Generate();

        var refreshToken = DomainRefreshToken.Create(user.Id, user.TenantId, tokenHash, refreshExpiresAt);
        await _refreshRepo.AddAsync(refreshToken, ct);
        await _refreshRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Audit: login bem-sucedido. TenantId={TenantId} UserId={UserId}",
            request.TenantId, user.Id);

        return Result<LoginDto>.Success(new LoginDto(
            accessToken,
            accessExpiresAt,
            user.Id,
            user.Name,
            user.Email,
            rawRefreshToken,
            refreshExpiresAt,
            user.IsAdmin));
    }
}
