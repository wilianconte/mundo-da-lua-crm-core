using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<bool> HasConflictAsync(Guid professionalId, DateTime startDateTime, DateTime endDateTime, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByProfessionalAsync(Guid tenantId, Guid professionalId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByPatientAsync(Guid tenantId, Guid patientId, CancellationToken ct = default);
}
