using MyCRM.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext
{
    private readonly ITenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantService tenant, ICurrentUserService currentUser)
        : base(options)
    {
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        modelBuilder.Entity<User>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Role>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IHasTenantId>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedBy = userId;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedBy = userId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

