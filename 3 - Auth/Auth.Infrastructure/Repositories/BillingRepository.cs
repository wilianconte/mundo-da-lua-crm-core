using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class BillingRepository : IBillingRepository
{
    private readonly AuthDbContext _db;

    public BillingRepository(AuthDbContext db) => _db = db;

    public async Task<Billing?> GetPendingByTenantPlanIdAsync(Guid tenantPlanId, CancellationToken ct = default) =>
        await _db.Billings
            .FirstOrDefaultAsync(x => x.TenantPlanId == tenantPlanId && x.Status == BillingStatus.Pending, ct);

    public async Task<IReadOnlyList<Billing>> GetAllPendingByTenantPlanIdAsync(Guid tenantPlanId, CancellationToken ct = default) =>
        await _db.Billings
            .Where(x => x.TenantPlanId == tenantPlanId && x.Status == BillingStatus.Pending)
            .ToListAsync(ct);

    public async Task<Billing?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Billings.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Billing billing, CancellationToken ct = default) =>
        await _db.Billings.AddAsync(billing, ct);

    public void Update(Billing billing) =>
        _db.Billings.Update(billing);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
