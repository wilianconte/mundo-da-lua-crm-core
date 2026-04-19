using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IProfessionalRepository : IRepository<Professional>
{
    Task<bool> PersonAlreadyActiveProfessionalAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default);
    Task<Professional?> GetByPersonIdAsync(Guid tenantId, Guid personId, CancellationToken ct = default);
}
