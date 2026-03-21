using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Shared.Kernel.Entities;

public abstract class TenantEntity : BaseEntity, IHasTenantId
{
    public Guid TenantId { get; set; }
}
