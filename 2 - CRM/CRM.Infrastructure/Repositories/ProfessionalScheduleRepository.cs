using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ProfessionalScheduleRepository : IProfessionalScheduleRepository
{
    private readonly CRMDbContext _db;
    public ProfessionalScheduleRepository(CRMDbContext db) => _db = db;

    public IQueryable<ProfessionalSchedule> Query() => _db.ProfessionalSchedules.AsNoTracking();
    public async Task<IReadOnlyList<ProfessionalSchedule>> GetAllAsync(CancellationToken ct = default) =>
        await _db.ProfessionalSchedules.AsNoTracking().ToListAsync(ct);
    public async Task<ProfessionalSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.ProfessionalSchedules.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(ProfessionalSchedule entity, CancellationToken ct = default) =>
        await _db.ProfessionalSchedules.AddAsync(entity, ct);
    public void Update(ProfessionalSchedule entity) => _db.ProfessionalSchedules.Update(entity);
    public void Delete(ProfessionalSchedule entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<bool> DayAlreadyScheduledAsync(Guid professionalId, int dayOfWeek, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.ProfessionalSchedules.AnyAsync(
            x => x.ProfessionalId == professionalId && x.DayOfWeek == dayOfWeek && (excludeId == null || x.Id != excludeId), ct);

    public async Task<IReadOnlyList<ProfessionalSchedule>> GetByProfessionalAsync(Guid professionalId, CancellationToken ct = default) =>
        await _db.ProfessionalSchedules.AsNoTracking()
            .Where(x => x.ProfessionalId == professionalId)
            .OrderBy(x => x.DayOfWeek)
            .ToListAsync(ct);
}
