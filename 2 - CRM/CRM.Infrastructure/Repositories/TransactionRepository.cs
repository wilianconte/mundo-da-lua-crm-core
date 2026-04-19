using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly CRMDbContext _db;

    public TransactionRepository(CRMDbContext db) => _db = db;

    public IQueryable<Transaction> Query() => _db.Transactions.AsNoTracking();

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Transactions.AsNoTracking().ToListAsync(ct);

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Transactions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Transaction entity, CancellationToken ct = default) =>
        await _db.Transactions.AddAsync(entity, ct);

    public void Update(Transaction entity) => _db.Transactions.Update(entity);

    public void Delete(Transaction entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
