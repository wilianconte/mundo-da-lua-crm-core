using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly CRMDbContext _db;

    public EmployeeRepository(CRMDbContext db) => _db = db;

    public IQueryable<Employee> Query() => _db.Employees.AsNoTracking();

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Employees.AsNoTracking().ToListAsync(ct);

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Employee entity, CancellationToken ct = default) =>
        await _db.Employees.AddAsync(entity, ct);

    public void Update(Employee entity) => _db.Employees.Update(entity);

    public void Delete(Employee entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<bool> PersonAlreadyEmployedAsync(
        Guid tenantId, Guid personId, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Employees.AnyAsync(
            x => x.TenantId == tenantId
              && x.PersonId == personId
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<bool> EmployeeCodeExistsAsync(
        Guid tenantId, string employeeCode, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Employees.AnyAsync(
            x => x.TenantId == tenantId
              && x.EmployeeCode == employeeCode.Trim()
              && (excludeId == null || x.Id != excludeId), ct);
}
