using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IProfessionalServiceRepository : IRepository<ProfessionalService>
{
    Task<bool> LinkExistsAsync(Guid professionalId, Guid serviceId, Guid? excludeId = null, CancellationToken ct = default);
}
