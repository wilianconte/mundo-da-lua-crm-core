using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<bool> PersonAlreadyEnrolledAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default);
    Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
