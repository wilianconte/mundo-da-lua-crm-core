using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _db;

    public RoleRepository(AuthDbContext db) => _db = db;

    public IQueryable<Role> Query() => _db.Roles.AsNoTracking();

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Roles.AsNoTracking().ToListAsync(ct);

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Role entity, CancellationToken ct = default) =>
        await _db.Roles.AddAsync(entity, ct);

    public void Update(Role entity) => _db.Roles.Update(entity);

    public void Delete(Role entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> NameExistsAsync(
        Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Roles.AnyAsync(
            x => x.TenantId == tenantId
              && x.Name == name.Trim()
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default) =>
        await _db.Roles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Name == name.Trim(), ct);
}
