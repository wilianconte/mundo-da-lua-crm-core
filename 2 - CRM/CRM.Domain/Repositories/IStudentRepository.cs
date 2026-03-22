using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<bool> PersonAlreadyEnrolledAsync(Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> RegistrationNumberExistsAsync(Guid tenantId, string registrationNumber, Guid? excludeId = null, CancellationToken ct = default);
}
