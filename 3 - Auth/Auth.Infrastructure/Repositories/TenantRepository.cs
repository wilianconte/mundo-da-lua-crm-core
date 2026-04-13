using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AuthDbContext _db;

    public TenantRepository(AuthDbContext db) => _db = db;

    public IQueryable<Tenant> Query() => _db.Tenants.AsQueryable();

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Tenants.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Tenants.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Tenants.AnyAsync(x =>
            !x.IsDeleted &&
            x.Name == name &&
            (excludeId == null || x.Id != excludeId.Value), ct);

    public async Task AddAsync(Tenant entity, CancellationToken ct = default) =>
        await _db.Tenants.AddAsync(entity, ct);

    public void Update(Tenant entity) => _db.Tenants.Update(entity);

    public void Delete(Tenant entity) => entity.SoftDelete();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
