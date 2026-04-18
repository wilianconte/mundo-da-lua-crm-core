using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly CRMDbContext _db;

    public PaymentMethodRepository(CRMDbContext db) => _db = db;

    public IQueryable<PaymentMethod> Query() => _db.PaymentMethods.AsNoTracking();

    public async Task<IReadOnlyList<PaymentMethod>> GetAllAsync(CancellationToken ct = default) =>
        await _db.PaymentMethods.AsNoTracking().ToListAsync(ct);

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(PaymentMethod entity, CancellationToken ct = default) =>
        await _db.PaymentMethods.AddAsync(entity, ct);

    public void Update(PaymentMethod entity) => _db.PaymentMethods.Update(entity);

    public void Delete(PaymentMethod entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
