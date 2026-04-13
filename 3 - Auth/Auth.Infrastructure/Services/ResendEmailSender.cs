using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using ResendEmail = Resend.EmailMessage;
using KernelEmail = MyCRM.Shared.Kernel.Services.EmailMessage;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.Auth.Infrastructure.Services;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(IResend resend, IOptions<ResendSettings> options, ILogger<ResendEmailSender> logger)
    {
        _resend   = resend;
        _settings = options.Value;
        _logger   = logger;
    }

    public async Task<EmailSendResult> SendAsync(KernelEmail message, CancellationToken ct = default)
    {
        try
        {
            var fromEmail = message.From ?? _settings.FromEmail;
            var fromName  = message.FromName ?? _settings.FromName;
            var fromAddress = string.IsNullOrWhiteSpace(fromName)
                ? fromEmail
                : $"{fromName} <{fromEmail}>";

            var toAddress = string.IsNullOrWhiteSpace(message.ToName)
                ? message.To
                : $"{message.ToName} <{message.To}>";

            var resendEmail = new ResendEmail
            {
                From     = fromAddress,
                To       = { toAddress },
                Subject  = message.Subject,
                HtmlBody = message.HtmlBody,
                TextBody = message.TextBody
            };

            await _resend.EmailSendAsync(resendEmail, ct);

            return EmailSendResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email para {To}: {Error}", message.To, ex.Message);
            return EmailSendResult.Failure("EMAIL_SEND_FAILED", ex.Message);
        }
    }
}
