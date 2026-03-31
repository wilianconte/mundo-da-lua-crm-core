using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.Auth.Domain.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionNamesByRoleIdsAsync(
        IReadOnlyCollection<Guid> roleIds, CancellationToken ct = default);
    Task<Permission?> GetByNameAsync(string name, CancellationToken ct = default);
}
