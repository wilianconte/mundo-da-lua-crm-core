using MyCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ITenantService _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Customer>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
