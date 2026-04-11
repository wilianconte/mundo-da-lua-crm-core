using MyCRM.Auth.Application.Commands.Tenants.UpdateTenant;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class UpdateTenantHandlerTests
{
    private readonly ITenantRepository _repository = Substitute.For<ITenantRepository>();
    private readonly UpdateTenantHandler _handler;

    public UpdateTenantHandlerTests()
    {
        _handler = new UpdateTenantHandler(_repository);
    }

    private static Tenant CreateTenant(string name = "Acme Ltda") =>
        Tenant.Create(name, Guid.NewGuid());

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsSuccessWithUpdatedValues()
    {
        var tenant = CreateTenant("Nome Antigo");
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync("Nome Novo", tenant.Id, default).Returns(false);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, "Nome Novo", TenantPlan.Basic, TenantStatus.Active),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nome Novo", result.Value!.Name);
        Assert.Equal(TenantPlan.Basic, result.Value.Plan);
        Assert.Equal(TenantStatus.Active, result.Value.Status);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Tenant?)null);

        var result = await _handler.Handle(
            new UpdateTenantCommand(Guid.NewGuid(), "Qualquer", TenantPlan.Free, TenantStatus.Active),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        var tenant = CreateTenant("Original");
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync("Duplicado", tenant.Id, default).Returns(true);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, "Duplicado", TenantPlan.Free, TenantStatus.Active),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_NAME_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UpdateStatusToActive_SetsActiveStatus()
    {
        var tenant = CreateTenant();
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync(Arg.Any<string>(), tenant.Id, default).Returns(false);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, tenant.Name, TenantPlan.Free, TenantStatus.Active),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Active, result.Value!.Status);
    }

    [Fact]
    public async Task Handle_UpdateStatusToSuspended_SetsSuspendedStatus()
    {
        var tenant = CreateTenant();
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync(Arg.Any<string>(), tenant.Id, default).Returns(false);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, tenant.Name, TenantPlan.Free, TenantStatus.Suspended),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Suspended, result.Value!.Status);
    }

    [Fact]
    public async Task Handle_UpdateStatusToCancelled_SetsCancelledStatus()
    {
        var tenant = CreateTenant();
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync(Arg.Any<string>(), tenant.Id, default).Returns(false);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, tenant.Name, TenantPlan.Free, TenantStatus.Cancelled),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Cancelled, result.Value!.Status);
    }

    [Fact]
    public async Task Handle_UpdatePlan_UpdatesPlan()
    {
        var tenant = CreateTenant();
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync(Arg.Any<string>(), tenant.Id, default).Returns(false);

        var result = await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, tenant.Name, TenantPlan.Premium, TenantStatus.Active),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlan.Premium, result.Value!.Plan);
    }

    [Fact]
    public async Task Handle_ValidUpdate_CallsRepositoryUpdateAndSave()
    {
        var tenant = CreateTenant();
        _repository.GetByIdAsync(tenant.Id, default).Returns(tenant);
        _repository.NameExistsAsync(Arg.Any<string>(), tenant.Id, default).Returns(false);

        await _handler.Handle(
            new UpdateTenantCommand(tenant.Id, "Novo Nome", TenantPlan.Basic, TenantStatus.Active),
            default);

        _repository.Received(1).Update(tenant);
        await _repository.Received(1).SaveChangesAsync(default);
    }
}
