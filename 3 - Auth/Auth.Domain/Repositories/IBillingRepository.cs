using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Domain.Repositories;

public interface IBillingRepository
{
    Task<Billing?> GetPendingByTenantPlanIdAsync(Guid tenantPlanId, CancellationToken ct = default);
    Task<IReadOnlyList<Billing>> GetAllPendingByTenantPlanIdAsync(Guid tenantPlanId, CancellationToken ct = default);
    Task<Billing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Billing billing, CancellationToken ct = default);
    void Update(Billing billing);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
