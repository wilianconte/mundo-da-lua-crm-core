using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.Auth.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByIdsIgnoringQueryFiltersAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken ct = default);
}
