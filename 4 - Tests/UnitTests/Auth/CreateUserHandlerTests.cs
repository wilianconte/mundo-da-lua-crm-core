using MyCRM.Auth.Application.Commands.Users.CreateUser;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class CreateUserHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly CreateUserHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateUserHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new CreateUserHandler(_repository, _passwordHasher, _tenant);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        _repository.EmailExistsAsync(_tenantId, "maria@test.com", default).Returns(false);
        _repository.PersonIdAlreadyLinkedAsync(_tenantId, Arg.Any<Guid>(), default).Returns(false);
        _passwordHasher.Hash("Password123!").Returns("hashed-password");

        var command = new CreateUserCommand("Maria", "maria@test.com", "Password123!", Guid.NewGuid());

        var result = await _handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Maria", result.Value.Name);
        Assert.Equal("maria@test.com", result.Value.Email);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        _repository.EmailExistsAsync(_tenantId, "existing@test.com", default).Returns(true);

        var command = new CreateUserCommand("Maria", "existing@test.com", "Password123!", null);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_EMAIL_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_PersonAlreadyLinked_ReturnsFailure()
    {
        var personId = Guid.NewGuid();
        _repository.EmailExistsAsync(_tenantId, "maria@test.com", default).Returns(false);
        _repository.PersonIdAlreadyLinkedAsync(_tenantId, personId, default).Returns(true);

        var command = new CreateUserCommand("Maria", "maria@test.com", "Password123!", personId);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_PERSON_ALREADY_LINKED", result.ErrorCode);
    }
}
