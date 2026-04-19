using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly CRMDbContext _db;
    public PatientRepository(CRMDbContext db) => _db = db;

    public IQueryable<Patient> Query() => _db.Patients.AsNoTracking();
    public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Patients.AsNoTracking().ToListAsync(ct);
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Patients.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Patient entity, CancellationToken ct = default) =>
        await _db.Patients.AddAsync(entity, ct);
    public void Update(Patient entity) => _db.Patients.Update(entity);
    public void Delete(Patient entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> PersonAlreadyActivePatientAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Patients.AnyAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && (excludeId == null || x.Id != excludeId), ct);

    public async Task<Patient?> GetByPersonIdAsync(Guid tenantId, Guid personId, CancellationToken ct = default) =>
        await _db.Patients.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, ct);
}
