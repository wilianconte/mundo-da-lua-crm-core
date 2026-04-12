using MyCRM.Auth.Application.Services;
using Microsoft.Extensions.Configuration;

namespace MyCRM.Auth.Infrastructure.Services;

public sealed class PasswordResetSettings : IPasswordResetSettings
{
    public string FrontendBaseUrl { get; }

    public PasswordResetSettings(IConfiguration configuration)
    {
        FrontendBaseUrl = configuration["FrontendBaseUrl"] ?? "https://app.mundodalua.com.br";
    }
}
