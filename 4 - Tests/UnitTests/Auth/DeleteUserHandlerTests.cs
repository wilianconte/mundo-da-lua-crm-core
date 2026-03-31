using MyCRM.Auth.Application.Commands.Users.DeleteUser;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class DeleteUserHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly DeleteUserHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteUserHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new DeleteUserHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnSuccess()
    {
        var user = User.Create(_tenantId, "Maria", "maria@test.com", "hashed");
        _repository.GetByIdAsync(user.Id, default).Returns(user);

        var result = await _handler.Handle(new DeleteUserCommand(user.Id), default);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Delete(user);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUserNotFoundError()
    {
        var unknownId = Guid.NewGuid();
        _repository.GetByIdAsync(unknownId, default).Returns((User?)null);

        var result = await _handler.Handle(new DeleteUserCommand(unknownId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_NOT_FOUND", result.ErrorCode);
        _repository.DidNotReceive().Delete(Arg.Any<User>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserFromDifferentTenant_ReturnsUserNotFoundError()
    {
        var otherTenantId = Guid.NewGuid();
        var user = User.Create(otherTenantId, "Carlos", "carlos@test.com", "hashed");
        _repository.GetByIdAsync(user.Id, default).Returns(user);

        var result = await _handler.Handle(new DeleteUserCommand(user.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_NOT_FOUND", result.ErrorCode);
        _repository.DidNotReceive().Delete(Arg.Any<User>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
