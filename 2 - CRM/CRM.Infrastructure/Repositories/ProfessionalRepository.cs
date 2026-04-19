using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ProfessionalRepository : IProfessionalRepository
{
    private readonly CRMDbContext _db;
    public ProfessionalRepository(CRMDbContext db) => _db = db;

    public IQueryable<Professional> Query() => _db.Professionals.AsNoTracking();
    public async Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Professionals.AsNoTracking().ToListAsync(ct);
    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Professionals.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Professional entity, CancellationToken ct = default) =>
        await _db.Professionals.AddAsync(entity, ct);
    public void Update(Professional entity) => _db.Professionals.Update(entity);
    public void Delete(Professional entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> PersonAlreadyActiveProfessionalAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Professionals.AnyAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && (excludeId == null || x.Id != excludeId), ct);

    public async Task<Professional?> GetByPersonIdAsync(Guid tenantId, Guid personId, CancellationToken ct = default) =>
        await _db.Professionals.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, ct);
}
