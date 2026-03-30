namespace MyCRM.Auth.Application.Services;

public interface IRefreshTokenGenerator
{
    /// <summary>
    /// Gera um triplo (token bruto, hash SHA-256, data de expiração).
    /// O token bruto deve ser enviado ao cliente; apenas o hash é persistido.
    /// </summary>
    (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) Generate();
}
