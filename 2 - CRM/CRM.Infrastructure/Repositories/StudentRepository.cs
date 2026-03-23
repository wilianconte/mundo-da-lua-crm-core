using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly CRMDbContext _db;

    public StudentRepository(CRMDbContext db) => _db = db;

    public IQueryable<Student> Query() => _db.Students.AsNoTracking();

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Students.AsNoTracking().ToListAsync(ct);

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Students.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Student entity, CancellationToken ct = default) =>
        await _db.Students.AddAsync(entity, ct);

    public void Update(Student entity) => _db.Students.Update(entity);

    public void Delete(Student entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> PersonAlreadyEnrolledAsync(
        Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Students.AnyAsync(
            x => x.TenantId == tenantId
              && x.PersonId == personId
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<bool> RegistrationNumberExistsAsync(
        Guid tenantId, string registrationNumber, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Students.AnyAsync(
            x => x.TenantId == tenantId
              && x.RegistrationNumber == registrationNumber.Trim()
              && (excludeId == null || x.Id != excludeId), ct);
}
