using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByDocumentNumberAsync(Guid tenantId, string documentNumber, CancellationToken ct = default);

    Task<bool> DocumentNumberExistsAsync(Guid tenantId, string documentNumber, Guid? excludeId = null, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(Guid tenantId, string email, Guid? excludeId = null, CancellationToken ct = default);
}
