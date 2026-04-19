using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly CRMDbContext _db;
    public ServiceRepository(CRMDbContext db) => _db = db;

    public IQueryable<Service> Query() => _db.Services.AsNoTracking();
    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Services.AsNoTracking().ToListAsync(ct);
    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Services.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Service entity, CancellationToken ct = default) =>
        await _db.Services.AddAsync(entity, ct);
    public void Update(Service entity) => _db.Services.Update(entity);
    public void Delete(Service entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Services.AnyAsync(
            x => x.TenantId == tenantId && x.Name == name && (excludeId == null || x.Id != excludeId), ct);
}
