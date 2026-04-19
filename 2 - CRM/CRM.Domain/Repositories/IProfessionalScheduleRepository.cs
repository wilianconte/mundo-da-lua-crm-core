using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IProfessionalScheduleRepository : IRepository<ProfessionalSchedule>
{
    Task<bool> DayAlreadyScheduledAsync(Guid professionalId, int dayOfWeek, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProfessionalSchedule>> GetByProfessionalAsync(Guid professionalId, CancellationToken ct = default);
}
