using MediatR;
using Microsoft.Extensions.Logging;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.Auth.Application.Commands.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, Result<bool>>
{
    private readonly IUserRepository _repo;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordResetSettings _settings;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IUserRepository repo,
        IEmailSender emailSender,
        IPasswordResetSettings settings,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _repo      = repo;
        _emailSender = emailSender;
        _settings  = settings;
        _logger    = logger;
    }

    public async Task<Result<bool>> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        // RN-033.1 — nunca revelar se o email existe ou não
        var user = await _repo.GetByEmailAcrossTenantsAsync(request.Email.ToLowerInvariant(), ct);
        if (user is null)
            return Result<bool>.Success(true);

        var token = Guid.NewGuid().ToString("N");
        user.SetPasswordResetToken(token, DateTime.UtcNow.AddHours(1));
        _repo.Update(user);
        await _repo.SaveChangesAsync(ct);

        var resetLink = $"{_settings.FrontendBaseUrl}/redefinir-senha?token={token}";

        var htmlBody = $"""
            <h2>Recuperação de senha — Mundo da Lua CRM</h2>
            <p>Olá, <strong>{user.Name}</strong>!</p>
            <p>Recebemos uma solicitação para redefinir a senha da sua conta.</p>
            <p>Clique no botão abaixo para criar uma nova senha:</p>
            <p>
              <a href="{resetLink}"
                 style="display:inline-block;padding:12px 24px;background-color:#4F46E5;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:bold;">
                Redefinir minha senha
              </a>
            </p>
            <p>Ou copie e cole o link abaixo no seu navegador:</p>
            <p><a href="{resetLink}">{resetLink}</a></p>
            <p><strong>Este link expira em 1 hora.</strong></p>
            <p>Se você não solicitou a redefinição de senha, ignore este e-mail com segurança.</p>
            """;

        try
        {
            await _emailSender.SendAsync(new EmailMessage(
                To:       user.Email,
                Subject:  "Recuperação de senha — Mundo da Lua CRM",
                HtmlBody: htmlBody,
                ToName:   user.Name), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar email de recuperação de senha para {Email}", user.Email);
        }

        return Result<bool>.Success(true);
    }
}
