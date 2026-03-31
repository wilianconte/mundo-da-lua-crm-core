using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class UpdateRoleHandlerTests
{
    private readonly IRoleRepository _repository = Substitute.For<IRoleRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly UpdateRoleHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateRoleHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new UpdateRoleHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsSuccess()
    {
        var role = Role.Create(_tenantId, "Antigo", null);
        _repository.GetByIdAsync(role.Id, default).Returns(role);
        _repository.NameExistsAsync(_tenantId, "Novo", role.Id, default).Returns(false);

        var result = await _handler.Handle(new UpdateRoleCommand(role.Id, "Novo", "Nova descrição", null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Novo", result.Value!.Name);
        Assert.Equal("Nova descrição", result.Value.Description);
    }

    [Fact]
    public async Task Handle_RoleNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((MyCRM.Auth.Domain.Entities.Role?)null);

        var result = await _handler.Handle(new UpdateRoleCommand(Guid.NewGuid(), "X", null, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        var role = Role.Create(_tenantId, "Original", null);
        _repository.GetByIdAsync(role.Id, default).Returns(role);
        _repository.NameExistsAsync(_tenantId, "Existente", role.Id, default).Returns(true);

        var result = await _handler.Handle(new UpdateRoleCommand(role.Id, "Existente", null, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NAME_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DeactivateRole_ReturnsInactive()
    {
        var role = Role.Create(_tenantId, "Ativo", null);
        _repository.GetByIdAsync(role.Id, default).Returns(role);
        _repository.NameExistsAsync(_tenantId, "Ativo", role.Id, default).Returns(false);

        var result = await _handler.Handle(new UpdateRoleCommand(role.Id, "Ativo", null, IsActive: false), default);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task Handle_WrongTenant_ReturnsNotFound()
    {
        var role = Role.Create(Guid.NewGuid(), "Role", null); // different tenant
        _repository.GetByIdAsync(role.Id, default).Returns(role);

        var result = await _handler.Handle(new UpdateRoleCommand(role.Id, "Role", null, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NOT_FOUND", result.ErrorCode);
    }
}
