using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class PersonRepository : IPersonRepository
{
    private readonly CRMDbContext _db;

    public PersonRepository(CRMDbContext db) => _db = db;

    public IQueryable<Person> Query() => _db.People.AsNoTracking();

    public async Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.People.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Person entity, CancellationToken ct = default) =>
        await _db.People.AddAsync(entity, ct);

    public void Update(Person entity) => _db.People.Update(entity);

    public void Delete(Person entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<Person?> GetByDocumentNumberAsync(
        Guid tenantId, string documentNumber, CancellationToken ct = default) =>
        await _db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.DocumentNumber == documentNumber.Trim(), ct);

    public async Task<bool> DocumentNumberExistsAsync(
        Guid tenantId, string documentNumber, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.People.AnyAsync(
            x => x.TenantId == tenantId
              && x.DocumentNumber == documentNumber.Trim()
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<bool> EmailExistsAsync(
        Guid tenantId, string email, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.People.AnyAsync(
            x => x.TenantId == tenantId
              && x.Email == email.Trim().ToLowerInvariant()
              && (excludeId == null || x.Id != excludeId), ct);
}
