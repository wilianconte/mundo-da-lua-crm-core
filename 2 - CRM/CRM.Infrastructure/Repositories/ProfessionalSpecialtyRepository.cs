using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ProfessionalSpecialtyRepository : IProfessionalSpecialtyRepository
{
    private readonly CRMDbContext _db;
    public ProfessionalSpecialtyRepository(CRMDbContext db) => _db = db;

    public IQueryable<ProfessionalSpecialty> Query() => _db.ProfessionalSpecialties.AsNoTracking();
    public async Task<IReadOnlyList<ProfessionalSpecialty>> GetAllAsync(CancellationToken ct = default) =>
        await _db.ProfessionalSpecialties.AsNoTracking().ToListAsync(ct);
    public async Task<ProfessionalSpecialty?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialties.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ProfessionalSpecialty entity, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialties.AddAsync(entity, ct);
    public void Update(ProfessionalSpecialty entity) => _db.ProfessionalSpecialties.Update(entity);
    public void Delete(ProfessionalSpecialty entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialties.AnyAsync(
            x => x.TenantId == tenantId && x.Name == name && (excludeId == null || x.Id != excludeId), ct);
}
