using MyCRM.Auth.Application.Commands.Roles.DeleteRole;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class DeleteRoleHandlerTests
{
    private readonly IRoleRepository _repository = Substitute.For<IRoleRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly DeleteRoleHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteRoleHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new DeleteRoleHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ExistingRole_ReturnsSuccess()
    {
        var role = Role.Create(_tenantId, "Admin", null);
        _repository.GetByIdAsync(role.Id, default).Returns(role);

        var result = await _handler.Handle(new DeleteRoleCommand(role.Id), default);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Delete(role);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_RoleNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Role?)null);

        var result = await _handler.Handle(new DeleteRoleCommand(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WrongTenant_ReturnsNotFound()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin", null); // different tenant
        _repository.GetByIdAsync(role.Id, default).Returns(role);

        var result = await _handler.Handle(new DeleteRoleCommand(role.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NOT_FOUND", result.ErrorCode);
    }
}
