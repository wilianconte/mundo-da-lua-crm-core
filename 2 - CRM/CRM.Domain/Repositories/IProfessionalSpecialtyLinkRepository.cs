using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IProfessionalSpecialtyLinkRepository : IRepository<ProfessionalSpecialtyLink>
{
    Task<bool> LinkExistsAsync(Guid professionalId, Guid specialtyId, CancellationToken ct = default);
    Task<int> CountByProfessionalAsync(Guid professionalId, CancellationToken ct = default);
}
