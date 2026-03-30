using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MyCRM.Auth.Application.Services;

namespace MyCRM.Auth.Infrastructure.Services;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly JwtSettings _settings;

    public RefreshTokenGenerator(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(bytes);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        var tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpiresInDays);
        return (rawToken, tokenHash, expiresAt);
    }
}
