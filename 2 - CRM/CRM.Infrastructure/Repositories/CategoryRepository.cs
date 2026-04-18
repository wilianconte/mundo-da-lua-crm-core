using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.CRM.Infrastructure.Persistence;

namespace MyCRM.CRM.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CRMDbContext _db;

    public CategoryRepository(CRMDbContext db) => _db = db;

    public IQueryable<Category> Query() => _db.FinancialCategories.AsNoTracking();

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _db.FinancialCategories.AsNoTracking().ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.FinancialCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Category entity, CancellationToken ct = default) =>
        await _db.FinancialCategories.AddAsync(entity, ct);

    public void Update(Category entity) => _db.FinancialCategories.Update(entity);

    public void Delete(Category entity) => entity.SoftDelete();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
