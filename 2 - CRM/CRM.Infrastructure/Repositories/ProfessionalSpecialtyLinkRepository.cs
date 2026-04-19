using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ProfessionalSpecialtyLinkRepository : IProfessionalSpecialtyLinkRepository
{
    private readonly CRMDbContext _db;
    public ProfessionalSpecialtyLinkRepository(CRMDbContext db) => _db = db;

    public IQueryable<ProfessionalSpecialtyLink> Query() => _db.ProfessionalSpecialtyLinks.AsNoTracking();
    public async Task<IReadOnlyList<ProfessionalSpecialtyLink>> GetAllAsync(CancellationToken ct = default) =>
        await _db.ProfessionalSpecialtyLinks.AsNoTracking().ToListAsync(ct);
    public async Task<ProfessionalSpecialtyLink?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialtyLinks.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ProfessionalSpecialtyLink entity, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialtyLinks.AddAsync(entity, ct);
    public void Update(ProfessionalSpecialtyLink entity) => _db.ProfessionalSpecialtyLinks.Update(entity);
    public void Delete(ProfessionalSpecialtyLink entity) => _db.ProfessionalSpecialtyLinks.Remove(entity);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> LinkExistsAsync(Guid professionalId, Guid specialtyId, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialtyLinks.AnyAsync(
            x => x.ProfessionalId == professionalId && x.SpecialtyId == specialtyId, ct);

    public async Task<int> CountByProfessionalAsync(Guid professionalId, CancellationToken ct = default) =>
        await _db.ProfessionalSpecialtyLinks.CountAsync(x => x.ProfessionalId == professionalId, ct);
}
