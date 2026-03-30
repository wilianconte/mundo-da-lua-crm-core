using Microsoft.Extensions.Logging.Abstractions;
using MyCRM.Auth.Application.Commands.RefreshToken;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class RefreshTokenHandlerTests
{
    private readonly IRefreshTokenRepository _refreshRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ITokenGenerator _tokenGen = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshGen = Substitute.For<IRefreshTokenGenerator>();
    private readonly RefreshTokenHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private const string RawToken = "valid-raw-token-for-testing";

    private static string ComputeHash(string token)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public RefreshTokenHandlerTests()
    {
        _tokenGen.Generate(Arg.Any<User>())
            .Returns(("new-access-token", DateTimeOffset.UtcNow.AddHours(1)));
        _refreshGen.Generate()
            .Returns(("new-raw-token", "new-hash-value", DateTimeOffset.UtcNow.AddDays(30)));

        _handler = new RefreshTokenHandler(
            _refreshRepo, _userRepo, _tokenGen, _refreshGen,
            NullLogger<RefreshTokenHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokensAndRevokesOld()
    {
        var hash = ComputeHash(RawToken);
        var stored = RefreshToken.Create(UserId, TenantId, hash, DateTimeOffset.UtcNow.AddDays(30));
        var user = User.Create(TenantId, "Test User", "user@test.com", "hashed");

        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(stored);
        _userRepo.GetByIdAsync(stored.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("new-access-token", result.Value!.Token);
        Assert.Equal("new-raw-token", result.Value.RefreshToken);
        Assert.True(stored.IsRevoked);
        await _refreshRepo.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _refreshRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsInvalidRefreshTokenError()
    {
        _refreshRepo.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
        await _refreshRepo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsInvalidRefreshTokenError()
    {
        var hash = ComputeHash(RawToken);
        var expired = RefreshToken.Create(UserId, TenantId, hash, DateTimeOffset.UtcNow.AddDays(-1));
        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(expired);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_RevokedToken_ReturnsInvalidRefreshTokenError()
    {
        var hash = ComputeHash(RawToken);
        var revoked = RefreshToken.Create(UserId, TenantId, hash, DateTimeOffset.UtcNow.AddDays(30));
        revoked.Revoke();
        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(revoked);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WrongTenant_ReturnsInvalidRefreshTokenError()
    {
        var hash = ComputeHash(RawToken);
        var otherTenant = Guid.NewGuid();
        var tokenOtherTenant = RefreshToken.Create(UserId, otherTenant, hash, DateTimeOffset.UtcNow.AddDays(30));
        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(tokenOtherTenant);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_REFRESH_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUserInactiveError()
    {
        var hash = ComputeHash(RawToken);
        var stored = RefreshToken.Create(UserId, TenantId, hash, DateTimeOffset.UtcNow.AddDays(30));
        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(stored);
        _userRepo.GetByIdAsync(stored.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_INACTIVE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUserInactiveError()
    {
        var hash = ComputeHash(RawToken);
        var stored = RefreshToken.Create(UserId, TenantId, hash, DateTimeOffset.UtcNow.AddDays(30));
        var user = User.Create(TenantId, "Test User", "user@test.com", "hashed");
        user.Deactivate();

        _refreshRepo.GetByTokenHashAsync(hash, Arg.Any<CancellationToken>()).Returns(stored);
        _userRepo.GetByIdAsync(stored.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new RefreshTokenCommand(TenantId, RawToken), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_INACTIVE", result.ErrorCode);
    }
}
