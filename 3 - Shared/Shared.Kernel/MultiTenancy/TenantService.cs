namespace MyCRM.Shared.Kernel.MultiTenancy;

public sealed class TenantService : ITenantService
{
    private Guid _tenantId;

    public Guid TenantId => _tenantId == Guid.Empty
        ? throw new InvalidOperationException("Tenant not set for the current request.")
        : _tenantId;

    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
}
