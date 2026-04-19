using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly CRMDbContext _db;
    public AppointmentRepository(CRMDbContext db) => _db = db;

    public IQueryable<Appointment> Query() => _db.Appointments.AsNoTracking();
    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Appointments.AsNoTracking().ToListAsync(ct);
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Appointment entity, CancellationToken ct = default) =>
        await _db.Appointments.AddAsync(entity, ct);
    public void Update(Appointment entity) => _db.Appointments.Update(entity);
    public void Delete(Appointment entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> HasConflictAsync(Guid professionalId, DateTime startDateTime, DateTime endDateTime, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Appointments.AnyAsync(
            x => x.ProfessionalId == professionalId
              && (excludeId == null || x.Id != excludeId)
              && x.StartDateTime < endDateTime
              && x.EndDateTime > startDateTime, ct);

    public async Task<IReadOnlyList<Appointment>> GetByProfessionalAsync(Guid tenantId, Guid professionalId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.ToDateTime(TimeOnly.MaxValue);
        return await _db.Appointments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ProfessionalId == professionalId
                     && x.StartDateTime >= start && x.StartDateTime <= end)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> GetByPatientAsync(Guid tenantId, Guid patientId, CancellationToken ct = default) =>
        await _db.Appointments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PatientId == patientId)
            .OrderByDescending(x => x.StartDateTime)
            .ToListAsync(ct);
}
