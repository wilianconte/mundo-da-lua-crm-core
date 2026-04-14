using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly AuthDbContext _db;

    public PlanRepository(AuthDbContext db) => _db = db;

    public IQueryable<Plan> Query() => _db.Plans.AsQueryable();

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Plans.AsNoTracking().ToListAsync(ct);

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Plans.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<Plan?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await _db.Plans.FirstOrDefaultAsync(x => x.Name == name.ToLowerInvariant() && !x.IsDeleted, ct);

    public async Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken ct = default) =>
        await _db.Plans.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).ToListAsync(ct);

    public async Task<Plan?> GetFreePlanAsync(CancellationToken ct = default) =>
        await _db.Plans.FirstOrDefaultAsync(x => x.Name == "free" && !x.IsDeleted, ct);

    public async Task<PlanFeature?> GetPlanFeatureAsync(Guid planId, string featureKey, CancellationToken ct = default) =>
        await _db.PlanFeatures
            .Include(x => x.Feature)
            .FirstOrDefaultAsync(x => x.PlanId == planId && x.Feature.Key == featureKey.ToLowerInvariant(), ct);

    public async Task AddAsync(Plan entity, CancellationToken ct = default) =>
        await _db.Plans.AddAsync(entity, ct);

    public void Update(Plan entity) => _db.Plans.Update(entity);

    public void Delete(Plan entity) => entity.SoftDelete();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
