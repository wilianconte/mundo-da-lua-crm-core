using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.CRM.Infrastructure.Persistence;

public sealed class CRMDbContext : DbContext
{
    private readonly ITenantService _tenant;

    public CRMDbContext(DbContextOptions<CRMDbContext> options, ITenantService tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Customer>        Customers        => Set<Customer>();
    public DbSet<Person>          People           => Set<Person>();
    public DbSet<Company>         Companies        => Set<Company>();
    public DbSet<Student>         Students         => Set<Student>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDbContext).Assembly);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Person>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Company>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Student>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<StudentGuardian>()
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

        foreach (var entry in ChangeTracker.Entries<Person>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }

        foreach (var entry in ChangeTracker.Entries<Company>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }

        foreach (var entry in ChangeTracker.Entries<Student>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }

        foreach (var entry in ChangeTracker.Entries<StudentGuardian>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
