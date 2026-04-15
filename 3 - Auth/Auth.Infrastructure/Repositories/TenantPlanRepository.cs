using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class TenantPlanRepository : ITenantPlanRepository
{
    private readonly AuthDbContext _db;

    public TenantPlanRepository(AuthDbContext db) => _db = db;

    public async Task<TenantPlan?> GetActiveByTenantIdAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.TenantPlans
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == TenantPlanStatus.Active, ct);

    public async Task<TenantPlan?> GetPausedByTenantIdAsync(Guid tenantId, CancellationToken ct = default) =>
        await _db.TenantPlans
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Status == TenantPlanStatus.Paused, ct);

    public async Task<bool> HasUsedTrialForPlanAsync(Guid tenantId, Guid planId, CancellationToken ct = default) =>
        await _db.TenantPlans
            .AnyAsync(x => x.TenantId == tenantId && x.PlanId == planId && x.IsTrial, ct);

    public async Task<TenantPlan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.TenantPlans
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TenantPlan tenantPlan, CancellationToken ct = default) =>
        await _db.TenantPlans.AddAsync(tenantPlan, ct);

    public void Update(TenantPlan tenantPlan) =>
        _db.TenantPlans.Update(tenantPlan);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
