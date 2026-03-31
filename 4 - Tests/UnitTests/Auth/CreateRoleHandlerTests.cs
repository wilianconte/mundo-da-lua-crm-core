using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class CreateRoleHandlerTests
{
    private readonly IRoleRepository _repository = Substitute.For<IRoleRepository>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly CreateRoleHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateRoleHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new CreateRoleHandler(_repository, _tenant);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        _repository.NameExistsAsync(_tenantId, "Administrador", null, default).Returns(false);

        var result = await _handler.Handle(new CreateRoleCommand("Administrador", "Acesso total"), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Administrador", result.Value.Name);
        Assert.Equal("Acesso total", result.Value.Description);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        _repository.NameExistsAsync(_tenantId, "Administrador", null, default).Returns(true);

        var result = await _handler.Handle(new CreateRoleCommand("Administrador", null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NAME_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_NullDescription_ReturnsSuccess()
    {
        _repository.NameExistsAsync(_tenantId, "Professor", null, default).Returns(false);

        var result = await _handler.Handle(new CreateRoleCommand("Professor", null), default);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Description);
    }
}
