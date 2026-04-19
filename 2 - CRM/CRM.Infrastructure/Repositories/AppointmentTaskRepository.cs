using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class AppointmentTaskRepository : IAppointmentTaskRepository
{
    private readonly CRMDbContext _db;
    public AppointmentTaskRepository(CRMDbContext db) => _db = db;

    public IQueryable<AppointmentTask> Query() => _db.AppointmentTasks.AsNoTracking();
    public async Task<IReadOnlyList<AppointmentTask>> GetAllAsync(CancellationToken ct = default) =>
        await _db.AppointmentTasks.AsNoTracking().ToListAsync(ct);
    public async Task<AppointmentTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.AppointmentTasks.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(AppointmentTask entity, CancellationToken ct = default) =>
        await _db.AppointmentTasks.AddAsync(entity, ct);
    public void Update(AppointmentTask entity) => _db.AppointmentTasks.Update(entity);
    public void Delete(AppointmentTask entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<AppointmentTask>> GetPendingByAppointmentAsync(Guid appointmentId, CancellationToken ct = default) =>
        await _db.AppointmentTasks.AsNoTracking()
            .Where(x => x.AppointmentId == appointmentId && x.Status == AppointmentTaskStatus.Pending)
            .ToListAsync(ct);
}
