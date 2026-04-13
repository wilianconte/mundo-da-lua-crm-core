using Microsoft.Extensions.Logging.Abstractions;
using MyCRM.Auth.Application.Commands.Auth.RequestPasswordReset;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MyCRM.UnitTests.Auth;

public sealed class RequestPasswordResetHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IPasswordResetSettings _settings = Substitute.For<IPasswordResetSettings>();
    private readonly RequestPasswordResetHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private const string Email = "user@test.com";

    public RequestPasswordResetHandlerTests()
    {
        _settings.FrontendBaseUrl.Returns("https://app.test.com");
        _handler = new RequestPasswordResetHandler(
            _repo,
            _emailSender,
            _settings,
            NullLogger<RequestPasswordResetHandler>.Instance);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsSuccessWithoutSendingEmail()
    {
        // RN-033.1 — não revelar existência do email
        _repo.GetByEmailAcrossTenantsAsync(Email, default).Returns((User?)null);

        var result = await _handler.Handle(new RequestPasswordResetCommand(Email), default);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserFound_SetsTokenAndSendsEmail()
    {
        var user = User.Create(TenantId, "Maria", Email, "hash");
        _repo.GetByEmailAcrossTenantsAsync(Email, default).Returns(user);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Success());

        var result = await _handler.Handle(new RequestPasswordResetCommand(Email), default);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.NotNull(user.PasswordResetToken);
        Assert.NotNull(user.PasswordResetTokenExpiresAt);
        _repo.Received(1).Update(user);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync(
            Arg.Is<EmailMessage>(m => m.To == Email && m.Subject.Contains("Recuperação")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailSendFails_StillReturnsSuccess()
    {
        var user = User.Create(TenantId, "Maria", Email, "hash");
        _repo.GetByEmailAcrossTenantsAsync(Email, default).Returns(user);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("SMTP error"));

        var result = await _handler.Handle(new RequestPasswordResetCommand(Email), default);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Handle_TokenExpiresInOneHour()
    {
        var before = DateTime.UtcNow.AddHours(1).AddSeconds(-5);
        var user = User.Create(TenantId, "Maria", Email, "hash");
        _repo.GetByEmailAcrossTenantsAsync(Email, default).Returns(user);
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(EmailSendResult.Success());

        await _handler.Handle(new RequestPasswordResetCommand(Email), default);

        Assert.NotNull(user.PasswordResetTokenExpiresAt);
        Assert.True(user.PasswordResetTokenExpiresAt > before);
    }
}
