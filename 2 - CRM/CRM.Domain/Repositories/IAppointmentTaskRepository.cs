using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IAppointmentTaskRepository : IRepository<AppointmentTask>
{
    Task<IReadOnlyList<AppointmentTask>> GetPendingByAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
}
