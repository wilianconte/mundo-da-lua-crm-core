using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<bool> PersonAlreadyActivePatientAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default);
    Task<Patient?> GetByPersonIdAsync(Guid tenantId, Guid personId, CancellationToken ct = default);
}
