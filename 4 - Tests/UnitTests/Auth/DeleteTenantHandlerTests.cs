using MyCRM.Auth.Application.Commands.Tenants.DeleteTenant;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class DeleteTenantHandlerTests
{
    private readonly ITenantRepository _repository = Substitute.For<ITenantRepository>();
    private readonly DeleteTenantHandler _handler;

    public DeleteTenantHandlerTests()
    {
        _handler = new DeleteTenantHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingTenant_ReturnsSuccess()
    {
        var tenant = Tenant.Create("Acme Ltda", Guid.NewGuid());
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);

        var result = await _handler.Handle(new DeleteTenantCommand(tenant.Id), default);

        Assert.True(result.IsSuccess);
        _repository.Received(1).Delete(tenant);
        await _repository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Tenant?)null);

        var result = await _handler.Handle(new DeleteTenantCommand(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_TenantNotFound_DoesNotCallDeleteOrSave()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Tenant?)null);

        await _handler.Handle(new DeleteTenantCommand(Guid.NewGuid()), default);

        _repository.DidNotReceive().Delete(Arg.Any<Tenant>());
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }
}
