using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _db;

    public PermissionRepository(AuthDbContext db) => _db = db;

    public IQueryable<Permission> Query() => _db.Permissions.AsNoTracking();

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Permissions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Permissions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await _db.Permissions.FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task<IReadOnlyList<string>> GetPermissionNamesByRoleIdsAsync(
        IReadOnlyCollection<Guid> roleIds, CancellationToken ct = default)
    {
        if (roleIds.Count == 0)
            return [];

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Permission entity, CancellationToken ct = default) =>
        await _db.Permissions.AddAsync(entity, ct);

    public void Update(Permission entity) => _db.Permissions.Update(entity);

    public void Delete(Permission entity) => _db.Permissions.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
