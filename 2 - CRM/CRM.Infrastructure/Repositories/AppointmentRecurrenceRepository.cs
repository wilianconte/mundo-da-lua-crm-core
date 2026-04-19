using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class AppointmentRecurrenceRepository : IAppointmentRecurrenceRepository
{
    private readonly CRMDbContext _db;
    public AppointmentRecurrenceRepository(CRMDbContext db) => _db = db;

    public IQueryable<AppointmentRecurrence> Query() => _db.AppointmentRecurrences.AsNoTracking();
    public async Task<IReadOnlyList<AppointmentRecurrence>> GetAllAsync(CancellationToken ct = default) =>
        await _db.AppointmentRecurrences.AsNoTracking().ToListAsync(ct);
    public async Task<AppointmentRecurrence?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.AppointmentRecurrences.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(AppointmentRecurrence entity, CancellationToken ct = default) =>
        await _db.AppointmentRecurrences.AddAsync(entity, ct);
    public void Update(AppointmentRecurrence entity) => _db.AppointmentRecurrences.Update(entity);
    public void Delete(AppointmentRecurrence entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<AppointmentRecurrence>> GetActiveByParentAsync(Guid parentAppointmentId, CancellationToken ct = default) =>
        await _db.AppointmentRecurrences.AsNoTracking()
            .Where(x => x.ParentAppointmentId == parentAppointmentId)
            .ToListAsync(ct);
}
