using MyCRM.Auth.Application.Commands.RefreshToken;

namespace MyCRM.UnitTests.Auth;

public sealed class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = new RefreshTokenCommand(Guid.NewGuid(), new string('a', 88));

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyTenantId_FailsValidation()
    {
        var command = new RefreshTokenCommand(Guid.Empty, "valid-token");

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenCommand.TenantId));
    }

    [Fact]
    public async Task Validate_EmptyRefreshToken_FailsValidation()
    {
        var command = new RefreshTokenCommand(Guid.NewGuid(), string.Empty);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }

    [Fact]
    public async Task Validate_RefreshTokenOver512Chars_FailsValidation()
    {
        var command = new RefreshTokenCommand(Guid.NewGuid(), new string('x', 513));

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenCommand.RefreshToken));
    }

    [Fact]
    public async Task Validate_RefreshTokenExactly512Chars_PassesValidation()
    {
        var command = new RefreshTokenCommand(Guid.NewGuid(), new string('x', 512));

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
