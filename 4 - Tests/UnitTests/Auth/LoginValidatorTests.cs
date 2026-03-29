using FluentValidation.TestHelper;
using MyCRM.Auth.Application.Commands.Login;

namespace MyCRM.UnitTests.Auth;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();
    private static readonly Guid ValidTenantId = Guid.NewGuid();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new LoginCommand(ValidTenantId, "user@test.com", "Password123!");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_FailsValidation()
    {
        var command = new LoginCommand(Guid.Empty, "user@test.com", "Password123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    public void InvalidEmail_FailsValidation(string email)
    {
        var command = new LoginCommand(ValidTenantId, email, "Password123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailTooLong_FailsValidation()
    {
        var longEmail = new string('a', 246) + "@test.com"; // 255 chars, exceeds 254 limit

        var command = new LoginCommand(ValidTenantId, longEmail, "Password123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmptyPassword_FailsValidation()
    {
        var command = new LoginCommand(ValidTenantId, "user@test.com", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void PasswordTooLong_FailsValidation()
    {
        var longPassword = new string('a', 129);

        var command = new LoginCommand(ValidTenantId, "user@test.com", longPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void PasswordAtMaxLength_PassesValidation()
    {
        var maxPassword = new string('a', 128);

        var command = new LoginCommand(ValidTenantId, "user@test.com", maxPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
