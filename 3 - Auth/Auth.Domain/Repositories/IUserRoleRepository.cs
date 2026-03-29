using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.Auth.Domain.Repositories;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<bool> UserHasRoleAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<UserRole>> GetUserRolesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<UserRole?> GetUserRoleAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default);
}
