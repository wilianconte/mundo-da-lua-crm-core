using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ProfessionalServiceRepository : IProfessionalServiceRepository
{
    private readonly CRMDbContext _db;
    public ProfessionalServiceRepository(CRMDbContext db) => _db = db;

    public IQueryable<ProfessionalService> Query() => _db.ProfessionalServices.AsNoTracking();
    public async Task<IReadOnlyList<ProfessionalService>> GetAllAsync(CancellationToken ct = default) =>
        await _db.ProfessionalServices.AsNoTracking().ToListAsync(ct);
    public async Task<ProfessionalService?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.ProfessionalServices.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ProfessionalService entity, CancellationToken ct = default) =>
        await _db.ProfessionalServices.AddAsync(entity, ct);
    public void Update(ProfessionalService entity) => _db.ProfessionalServices.Update(entity);
    public void Delete(ProfessionalService entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> LinkExistsAsync(Guid professionalId, Guid serviceId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.ProfessionalServices.AnyAsync(
            x => x.ProfessionalId == professionalId && x.ServiceId == serviceId && (excludeId == null || x.Id != excludeId), ct);
}
