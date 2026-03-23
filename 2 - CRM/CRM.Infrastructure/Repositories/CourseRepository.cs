using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class CourseRepository : ICourseRepository
{
    private readonly CRMDbContext _db;

    public CourseRepository(CRMDbContext db) => _db = db;

    public IQueryable<Course> Query() => _db.Courses.AsNoTracking();

    public async Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Courses.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Course entity, CancellationToken ct = default) =>
        await _db.Courses.AddAsync(entity, ct);

    public void Update(Course entity) => _db.Courses.Update(entity);

    public void Delete(Course entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> CodeExistsAsync(
        Guid tenantId, string code, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Courses.AnyAsync(
            x => x.TenantId == tenantId
              && x.Code == code.Trim()
              && (excludeId == null || x.Id != excludeId), ct);
}
