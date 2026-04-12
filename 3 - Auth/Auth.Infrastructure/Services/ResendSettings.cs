namespace MyCRM.Auth.Infrastructure.Services;

public sealed class ResendSettings
{
    public string ApiKey { get; init; } = default!;
    public string FromEmail { get; init; } = default!;
    public string FromName { get; init; } = "Mundo da Lua CRM";
}
