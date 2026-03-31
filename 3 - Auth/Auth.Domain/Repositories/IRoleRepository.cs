using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.Auth.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
}
