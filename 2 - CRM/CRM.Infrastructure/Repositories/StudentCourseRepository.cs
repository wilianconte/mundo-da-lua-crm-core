using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class StudentCourseRepository : IStudentCourseRepository
{
    private readonly CRMDbContext _db;

    public StudentCourseRepository(CRMDbContext db) => _db = db;

    public IQueryable<StudentCourse> Query() => _db.StudentCourses.AsNoTracking();

    public async Task<StudentCourse?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.StudentCourses.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(StudentCourse entity, CancellationToken ct = default) =>
        await _db.StudentCourses.AddAsync(entity, ct);

    public void Update(StudentCourse entity) => _db.StudentCourses.Update(entity);

    public void Delete(StudentCourse entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> ActiveEnrollmentExistsAsync(
        Guid tenantId, Guid studentId, Guid courseId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.StudentCourses.AnyAsync(
            x => x.TenantId == tenantId
              && x.StudentId == studentId
              && x.CourseId  == courseId
              && (x.Status == StudentCourseStatus.Active || x.Status == StudentCourseStatus.Pending)
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<IReadOnlyList<StudentCourse>> GetByStudentIdAsync(
        Guid studentId, CancellationToken ct = default) =>
        await _db.StudentCourses
            .Where(x => x.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StudentCourse>> GetByCourseIdAsync(
        Guid courseId, CancellationToken ct = default) =>
        await _db.StudentCourses
            .Where(x => x.CourseId == courseId)
            .AsNoTracking()
            .ToListAsync(ct);
}
