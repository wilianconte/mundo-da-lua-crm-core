using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class StudentGuardianRepository : IStudentGuardianRepository
{
    private readonly CRMDbContext _db;

    public StudentGuardianRepository(CRMDbContext db) => _db = db;

    public IQueryable<StudentGuardian> Query() => _db.StudentGuardians.AsNoTracking();

    public async Task<IReadOnlyList<StudentGuardian>> GetAllAsync(CancellationToken ct = default) =>
        await _db.StudentGuardians.AsNoTracking().ToListAsync(ct);

    public async Task<StudentGuardian?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.StudentGuardians.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(StudentGuardian entity, CancellationToken ct = default) =>
        await _db.StudentGuardians.AddAsync(entity, ct);

    public void Update(StudentGuardian entity) => _db.StudentGuardians.Update(entity);

    public void Delete(StudentGuardian entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> GuardianAlreadyLinkedAsync(
        Guid tenantId, Guid studentId, Guid guardianPersonId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.StudentGuardians.AnyAsync(
            x => x.TenantId == tenantId
              && x.StudentId == studentId
              && x.GuardianPersonId == guardianPersonId
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<IReadOnlyList<StudentGuardian>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default) =>
        await _db.StudentGuardians
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .ToListAsync(ct);
}
