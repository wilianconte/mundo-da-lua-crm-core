using FluentValidation.TestHelper;
using MyCRM.Auth.Application.Commands.Users.CreateUser;

namespace MyCRM.UnitTests.Auth;

public sealed class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new CreateUserCommand("Maria Souza", "maria@test.com", "Password123!", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyName_FailsValidation(string name)
    {
        var command = new CreateUserCommand(name, "maria@test.com", "Password123!", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("missing@")]
    public void InvalidEmail_FailsValidation(string email)
    {
        var command = new CreateUserCommand("Maria", email, "Password123!", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmptyPassword_FailsValidation()
    {
        var command = new CreateUserCommand("Maria", "maria@test.com", "", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
