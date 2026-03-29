using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AuthDbContext _db;

    public UserRoleRepository(AuthDbContext db) => _db = db;

    public IQueryable<UserRole> Query() => _db.UserRoles.AsNoTracking();

    public async Task<IReadOnlyList<UserRole>> GetAllAsync(CancellationToken ct = default) =>
        await _db.UserRoles.AsNoTracking().ToListAsync(ct);

    public async Task<UserRole?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.UserRoles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(UserRole entity, CancellationToken ct = default) =>
        await _db.UserRoles.AddAsync(entity, ct);

    public void Update(UserRole entity) => _db.UserRoles.Update(entity);

    public void Delete(UserRole entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> UserHasRoleAsync(
        Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default) =>
        await _db.UserRoles.AnyAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.RoleId == roleId, ct);

    public async Task<IReadOnlyList<UserRole>> GetUserRolesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default) =>
        await _db.UserRoles
            .Include(x => x.Role)
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<UserRole?> GetUserRoleAsync(
        Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default) =>
        await _db.UserRoles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.RoleId == roleId, ct);
}
