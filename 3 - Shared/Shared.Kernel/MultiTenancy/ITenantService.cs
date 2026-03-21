namespace MyCRM.Shared.Kernel.MultiTenancy;

public interface ITenantService
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}
