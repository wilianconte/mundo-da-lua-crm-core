using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.Auth.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _db;

    public RefreshTokenRepository(AuthDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await _db.RefreshTokens.AddAsync(token, ct);

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
