using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IStudentGuardianRepository : IRepository<StudentGuardian>
{
    Task<bool> GuardianAlreadyLinkedAsync(Guid tenantId, Guid studentId, Guid guardianPersonId, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<StudentGuardian>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
}
