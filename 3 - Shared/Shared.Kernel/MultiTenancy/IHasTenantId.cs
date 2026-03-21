namespace MyCRM.Shared.Kernel.MultiTenancy;

public interface IHasTenantId
{
    Guid TenantId { get; set; }
}
