using Microsoft.Extensions.Logging.Abstractions;
using MyCRM.Auth.Application.Commands.Auth.ResetPassword;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class ResetPasswordHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ResetPasswordHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private const string ValidToken = "abc123token";
    private const string NewPassword = "NewPassword123!";

    public ResetPasswordHandlerTests()
    {
        _hasher.Hash(NewPassword).Returns("new-hashed-password");
        _handler = new ResetPasswordHandler(
            _repo,
            _hasher,
            NullLogger<ResetPasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidToken_ResetsPasswordAndReturnsSuccess()
    {
        var user = CreateUserWithToken(DateTime.UtcNow.AddHours(1));
        _repo.GetByPasswordResetTokenAsync(ValidToken, default).Returns(user);

        var result = await _handler.Handle(
            new ResetPasswordCommand(ValidToken, NewPassword, NewPassword), default);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Null(user.PasswordResetToken);
        Assert.Null(user.PasswordResetTokenExpiresAt);
        _repo.Received(1).Update(user);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsInvalidTokenError()
    {
        _repo.GetByPasswordResetTokenAsync(ValidToken, default).Returns((User?)null);

        var result = await _handler.Handle(
            new ResetPasswordCommand(ValidToken, NewPassword, NewPassword), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_RESET_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsExpiredError()
    {
        var user = CreateUserWithToken(DateTime.UtcNow.AddHours(-1)); // expirado
        _repo.GetByPasswordResetTokenAsync(ValidToken, default).Returns(user);

        var result = await _handler.Handle(
            new ResetPasswordCommand(ValidToken, NewPassword, NewPassword), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("EXPIRED_RESET_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_PasswordMismatch_ReturnsValidationError()
    {
        var user = CreateUserWithToken(DateTime.UtcNow.AddHours(1));
        _repo.GetByPasswordResetTokenAsync(ValidToken, default).Returns(user);

        var result = await _handler.Handle(
            new ResetPasswordCommand(ValidToken, NewPassword, "DifferentPassword!"), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
        _repo.DidNotReceive().Update(Arg.Any<User>());
    }

    private static User CreateUserWithToken(DateTime expiresAt)
    {
        var user = User.Create(TenantId, "Test User", "user@test.com", "old-hash");
        user.SetPasswordResetToken(ValidToken, expiresAt);
        return user;
    }
}
