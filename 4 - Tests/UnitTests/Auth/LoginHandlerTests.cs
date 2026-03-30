using Microsoft.Extensions.Logging.Abstractions;
using MyCRM.Auth.Application.Commands.Login;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class LoginHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator _tokenGen = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshGen = Substitute.For<IRefreshTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly ILoginAttemptTracker _tracker = Substitute.For<ILoginAttemptTracker>();
    private readonly LoginHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private const string Email = "user@test.com";
    private const string Password = "Password123!";

    public LoginHandlerTests()
    {
        _refreshGen.Generate().Returns(("rawToken", "hashValue", DateTimeOffset.UtcNow.AddDays(30)));
        _handler = new LoginHandler(_repo, _hasher, _tokenGen, _refreshGen, _refreshRepo, _tracker,
            NullLogger<LoginHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessAndResetsTracker()
    {
        var user = CreateActiveUser();
        _tracker.IsLockedOut(Arg.Any<string>()).Returns(false);
        _repo.GetByEmailAsync(TenantId, Email, default).Returns(user);
        _hasher.Verify(Password, user.PasswordHash).Returns(true);
        _tokenGen.Generate(user).Returns(("token", DateTimeOffset.UtcNow.AddHours(1)));

        var result = await _handler.Handle(new LoginCommand(TenantId, Email, Password), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value!.Token);
        _tracker.Received(1).ResetFailures(Arg.Any<string>());
        _tracker.DidNotReceive().RecordFailure(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WrongPassword_RecordsFailureAndReturnsError()
    {
        var user = CreateActiveUser();
        _tracker.IsLockedOut(Arg.Any<string>()).Returns(false);
        _repo.GetByEmailAsync(TenantId, Email, default).Returns(user);
        _hasher.Verify(Password, user.PasswordHash).Returns(false);

        var result = await _handler.Handle(new LoginCommand(TenantId, Email, Password), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_CREDENTIALS", result.ErrorCode);
        _tracker.Received(1).RecordFailure(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_UserNotFound_RecordsFailureAndReturnsError()
    {
        _tracker.IsLockedOut(Arg.Any<string>()).Returns(false);
        _repo.GetByEmailAsync(TenantId, Email, default).Returns((User?)null);

        var result = await _handler.Handle(new LoginCommand(TenantId, Email, Password), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_CREDENTIALS", result.ErrorCode);
        _tracker.Received(1).RecordFailure(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_AccountLockedOut_ReturnsErrorWithoutCheckingCredentials()
    {
        _tracker.IsLockedOut(Arg.Any<string>()).Returns(true);

        var result = await _handler.Handle(new LoginCommand(TenantId, Email, Password), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("LOGIN_LOCKED_OUT", result.ErrorCode);
        await _repo.DidNotReceive().GetByEmailAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsErrorWithoutRecordingFailure()
    {
        var user = CreateInactiveUser();
        _tracker.IsLockedOut(Arg.Any<string>()).Returns(false);
        _repo.GetByEmailAsync(TenantId, Email, default).Returns(user);
        _hasher.Verify(Password, user.PasswordHash).Returns(true);

        var result = await _handler.Handle(new LoginCommand(TenantId, Email, Password), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_INACTIVE", result.ErrorCode);
        _tracker.DidNotReceive().RecordFailure(Arg.Any<string>());
    }

    private static User CreateActiveUser() =>
        User.Create(TenantId, "Test User", Email, "hashed");

    private static User CreateInactiveUser()
    {
        var user = User.Create(TenantId, "Test User", Email, "hashed");
        user.Deactivate();
        return user;
    }
}
