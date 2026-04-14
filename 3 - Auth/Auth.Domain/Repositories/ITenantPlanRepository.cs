using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Domain.Repositories;

public interface ITenantPlanRepository
{
    Task<TenantPlan?> GetActiveByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantPlan?> GetPausedByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> HasUsedTrialForPlanAsync(Guid tenantId, Guid planId, CancellationToken ct = default);
    Task<TenantPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TenantPlan tenantPlan, CancellationToken ct = default);
    void Update(TenantPlan tenantPlan);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
