using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.CRM.Infrastructure.Persistence;

public sealed class CRMDbContext : DbContext
{
    private readonly ITenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public CRMDbContext(DbContextOptions<CRMDbContext> options, ITenantService tenant, ICurrentUserService currentUser)
        : base(options)
    {
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public DbSet<Customer>        Customers        => Set<Customer>();
    public DbSet<Person>          People           => Set<Person>();
    public DbSet<Company>         Companies        => Set<Company>();
    public DbSet<Student>         Students         => Set<Student>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();
    public DbSet<Course>          Courses          => Set<Course>();
    public DbSet<StudentCourse>   StudentCourses   => Set<StudentCourse>();
    public DbSet<Employee>        Employees        => Set<Employee>();

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

        modelBuilder.Entity<Course>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<StudentCourse>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Employee>()
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

