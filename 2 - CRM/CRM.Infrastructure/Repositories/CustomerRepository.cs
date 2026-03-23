using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CRMDbContext _db;

    public CustomerRepository(CRMDbContext db) => _db = db;

    public IQueryable<Customer> Query() => _db.Customers.AsNoTracking();

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Customers.AsNoTracking().ToListAsync(ct);
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Customer entity, CancellationToken ct = default) =>
        await _db.Customers.AddAsync(entity, ct);
    public void Update(Customer entity) => _db.Customers.Update(entity);
    public void Delete(Customer entity) => entity.SoftDelete();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, Guid? excludeId = null, CancellationToken ct = default) =>
        await _db.Customers.AnyAsync(x => x.TenantId == tenantId && x.Email == email.ToLowerInvariant() && (excludeId == null || x.Id != excludeId), ct);
}
