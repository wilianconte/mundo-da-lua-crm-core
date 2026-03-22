using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByRegistrationNumberAsync(
        Guid tenantId,
        string registrationNumber,
        CancellationToken ct = default);

    Task<bool> RegistrationNumberExistsAsync(
        Guid tenantId,
        string registrationNumber,
        Guid? excludeId = null,
        CancellationToken ct = default);

    Task<bool> EmailExistsAsync(
        Guid tenantId,
        string email,
        Guid? excludeId = null,
        CancellationToken ct = default);
}
