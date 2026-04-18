using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository
{
    private readonly CRMDbContext _db;

    public WalletRepository(CRMDbContext db) => _db = db;

    public IQueryable<Wallet> Query() => _db.Wallets.AsNoTracking();

    public async Task<IReadOnlyList<Wallet>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Wallets.AsNoTracking().ToListAsync(ct);

    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Wallets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Wallet entity, CancellationToken ct = default) =>
        await _db.Wallets.AddAsync(entity, ct);

    public void Update(Wallet entity) => _db.Wallets.Update(entity);

    public void Delete(Wallet entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public async Task<decimal> GetCurrentBalanceAsync(Guid walletId, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == walletId, ct);

        if (wallet is null) return 0;

        var incomeTotal = await _db.Transactions
            .Where(t => t.WalletId == walletId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount, ct);

        var expenseTotal = await _db.Transactions
            .Where(t => t.WalletId == walletId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount, ct);

        return wallet.RecalculateBalance(incomeTotal, expenseTotal);
    }
}
