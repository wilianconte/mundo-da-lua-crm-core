using MediatR;
using Microsoft.Extensions.Logging;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using DomainRefreshToken = MyCRM.Auth.Domain.Entities.RefreshToken;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<LoginDto>>
{
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IUserRepository _userRepo;
    private readonly ITokenGenerator _tokenGen;
    private readonly IRefreshTokenGenerator _refreshGen;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshRepo,
        IUserRepository userRepo,
        ITokenGenerator tokenGen,
        IRefreshTokenGenerator refreshGen,
        ILogger<RefreshTokenHandler> logger)
    {
        _refreshRepo = refreshRepo;
        _userRepo = userRepo;
        _tokenGen = tokenGen;
        _refreshGen = refreshGen;
        _logger = logger;
    }

    public async Task<Result<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = ComputeHash(request.RefreshToken);
        var stored = await _refreshRepo.GetByTokenHashAsync(tokenHash, ct);

        if (stored is null || !stored.IsActive || stored.TenantId != request.TenantId)
        {
            _logger.LogWarning("Refresh token inválido ou expirado. TenantId={TenantId}", request.TenantId);
            return Result<LoginDto>.Failure("INVALID_REFRESH_TOKEN", "Refresh token inválido ou expirado.");
        }

        var user = await _userRepo.GetByIdAsync(stored.UserId, ct);

        if (user is null || !user.IsActive)
            return Result<LoginDto>.Failure("USER_INACTIVE", "Usuário inativo ou não encontrado.");

        // Rotação: revoga o token atual e emite um novo
        stored.Revoke();

        var (accessToken, accessExpiresAt) = _tokenGen.Generate(user);
        var (rawRefreshToken, newTokenHash, refreshExpiresAt) = _refreshGen.Generate();

        var newRefreshToken = DomainRefreshToken.Create(user.Id, user.TenantId, newTokenHash, refreshExpiresAt);
        await _refreshRepo.AddAsync(newRefreshToken, ct);
        await _refreshRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Audit: refresh token rotacionado. TenantId={TenantId} UserId={UserId}",
            request.TenantId, user.Id);

        return Result<LoginDto>.Success(new LoginDto(
            accessToken,
            accessExpiresAt,
            user.Id,
            user.Name,
            user.Email,
            rawRefreshToken,
            refreshExpiresAt));
    }

    private static string ComputeHash(string rawToken)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
