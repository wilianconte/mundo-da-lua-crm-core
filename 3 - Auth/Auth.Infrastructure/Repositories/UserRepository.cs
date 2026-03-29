using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db) => _db = db;

    public IQueryable<User> Query() => _db.Users.AsNoTracking();

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Users.AsNoTracking().ToListAsync(ct);
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(User entity, CancellationToken ct = default) =>
        await _db.Users.AddAsync(entity, ct);
    public void Update(User entity) => _db.Users.Update(entity);
    public void Delete(User entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default) =>
        await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email.ToLowerInvariant() && !x.IsDeleted, ct);
    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken ct = default) =>
        await _db.Users.AnyAsync(x => x.TenantId == tenantId && x.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> PersonIdAlreadyLinkedAsync(Guid tenantId, Guid personId, CancellationToken ct = default) =>
        await _db.Users.AnyAsync(x => x.TenantId == tenantId && x.PersonId == personId, ct);
}
