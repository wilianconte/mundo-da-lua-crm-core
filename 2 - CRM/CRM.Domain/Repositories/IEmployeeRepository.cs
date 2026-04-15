using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>Returns true if the given person already has an active (non-deleted) Employee record in the tenant.</summary>
    Task<bool> PersonAlreadyEmployedAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Returns true if the given employee code is already in use within the tenant.</summary>
    Task<bool> EmployeeCodeExistsAsync(Guid tenantId, string employeeCode, Guid? excludeId = null, CancellationToken ct = default);

    Task<int> CountActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
