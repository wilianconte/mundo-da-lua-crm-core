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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // ── Planos e assinaturas ──────────────────────────────────────────────────
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<TenantPlan> TenantPlans => Set<TenantPlan>();
    public DbSet<Billing> Billings => Set<Billing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        modelBuilder.Entity<User>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Role>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        // Match the Role global filter to avoid required-principal warnings.
        modelBuilder.Entity<UserRole>()
            .HasQueryFilter(x =>
                !x.User.IsDeleted &&
                !x.Role.IsDeleted &&
                x.User.TenantId == _tenant.TenantId &&
                x.Role.TenantId == _tenant.TenantId);

        // Match the Role global filter to avoid required-principal warnings.
        modelBuilder.Entity<RolePermission>()
            .HasQueryFilter(x =>
                !x.Role.IsDeleted &&
                x.Role.TenantId == _tenant.TenantId);

        // Tenant não é filtrado por TenantId (é a raiz do tenant); apenas soft-delete.
        modelBuilder.Entity<Tenant>()
            .HasQueryFilter(x => !x.IsDeleted);

        // Entidades de plano são globais — apenas soft-delete.
        modelBuilder.Entity<Plan>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Feature>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<PlanFeature>()
            .HasQueryFilter(x => !x.IsDeleted);

        // TenantPlan e Billing: sem query filter global por TenantId —
        // os repositórios filtram explicitamente via tenantId no parâmetro.
        modelBuilder.Entity<TenantPlan>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Billing>()
            .HasQueryFilter(x => !x.IsDeleted);

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
