namespace MyCRM.Auth.Application.DTOs;

public record LoginDto(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Name,
    string Email,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    bool IsAdmin);
