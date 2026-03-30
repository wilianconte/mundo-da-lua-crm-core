using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Token opaco de longa duração usado para emitir novos access tokens sem nova autenticação.
/// Armazenado como hash SHA-256 — o token bruto trafega apenas no response e nunca é persistido.
/// A cada uso (rotação), o token antigo é revogado e um novo é emitido.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>Hash SHA-256 do token bruto (hex lowercase).</summary>
    public string TokenHash { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, Guid tenantId, string tokenHash, DateTimeOffset expiresAt)
    {
        return new RefreshToken
        {
            UserId = userId,
            TenantId = tenantId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
