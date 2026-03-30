using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
