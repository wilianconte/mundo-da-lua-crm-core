namespace MyCRM.Auth.Infrastructure.Services;

public class JwtSettings
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiresInMinutes { get; set; } = 60;
    public int RefreshTokenExpiresInDays { get; set; } = 30;
}
