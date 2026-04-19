using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IAppointmentRecurrenceRepository : IRepository<AppointmentRecurrence>
{
    Task<IReadOnlyList<AppointmentRecurrence>> GetActiveByParentAsync(Guid parentAppointmentId, CancellationToken ct = default);
}
