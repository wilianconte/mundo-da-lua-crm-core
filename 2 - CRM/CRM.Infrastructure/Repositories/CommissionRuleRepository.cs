using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class CommissionRuleRepository : ICommissionRuleRepository
{
    private readonly CRMDbContext _db;
    public CommissionRuleRepository(CRMDbContext db) => _db = db;

    public IQueryable<CommissionRule> Query() => _db.CommissionRules.AsNoTracking();
    public async Task<IReadOnlyList<CommissionRule>> GetAllAsync(CancellationToken ct = default) =>
        await _db.CommissionRules.AsNoTracking().ToListAsync(ct);
    public async Task<CommissionRule?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.CommissionRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(CommissionRule entity, CancellationToken ct = default) =>
        await _db.CommissionRules.AddAsync(entity, ct);
    public void Update(CommissionRule entity) => _db.CommissionRules.Update(entity);
    public void Delete(CommissionRule entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<CommissionRule?> GetEffectiveRuleAsync(Guid tenantId, Guid professionalId, Guid serviceId, CancellationToken ct = default)
    {
        // Priority: professional+service > professional > tenant
        return await _db.CommissionRules
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProfessionalId != null && x.ServiceId != null)
            .ThenByDescending(x => x.ProfessionalId != null)
            .FirstOrDefaultAsync(
                x => (x.ProfessionalId == professionalId && x.ServiceId == serviceId)
                  || (x.ProfessionalId == professionalId && x.ServiceId == null)
                  || (x.ProfessionalId == null && x.ServiceId == null), ct);
    }
}
