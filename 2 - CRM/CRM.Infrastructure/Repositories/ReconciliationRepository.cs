using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class ReconciliationRepository : IReconciliationRepository
{
    private readonly CRMDbContext _db;

    public ReconciliationRepository(CRMDbContext db) => _db = db;

    public IQueryable<Reconciliation> Query() => _db.Reconciliations.AsNoTracking();

    public async Task<IReadOnlyList<Reconciliation>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Reconciliations.AsNoTracking().ToListAsync(ct);

    public async Task<Reconciliation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Reconciliations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Reconciliation entity, CancellationToken ct = default) =>
        await _db.Reconciliations.AddAsync(entity, ct);

    public void Update(Reconciliation entity) => _db.Reconciliations.Update(entity);

    public void Delete(Reconciliation entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
