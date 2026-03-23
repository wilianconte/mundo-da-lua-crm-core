using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly CRMDbContext _db;

    public CompanyRepository(CRMDbContext db) => _db = db;

    public IQueryable<Company> Query() => _db.Companies.AsNoTracking();

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Companies.AsNoTracking().ToListAsync(ct);

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Companies.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Company entity, CancellationToken ct = default) =>
        await _db.Companies.AddAsync(entity, ct);

    public void Update(Company entity) => _db.Companies.Update(entity);

    public void Delete(Company entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<Company?> GetByRegistrationNumberAsync(
        Guid tenantId, string registrationNumber, CancellationToken ct = default) =>
        await _db.Companies.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RegistrationNumber == registrationNumber.Trim(), ct);

    public async Task<bool> RegistrationNumberExistsAsync(
        Guid tenantId, string registrationNumber, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Companies.AnyAsync(
            x => x.TenantId == tenantId
              && x.RegistrationNumber == registrationNumber.Trim()
              && (excludeId == null || x.Id != excludeId), ct);

    public async Task<bool> EmailExistsAsync(
        Guid tenantId, string email, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Companies.AnyAsync(
            x => x.TenantId == tenantId
              && x.Email == email.Trim().ToLowerInvariant()
              && (excludeId == null || x.Id != excludeId), ct);
}
