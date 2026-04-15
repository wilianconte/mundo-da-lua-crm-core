using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface ICourseRepository : IRepository<Course>
{
    Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
