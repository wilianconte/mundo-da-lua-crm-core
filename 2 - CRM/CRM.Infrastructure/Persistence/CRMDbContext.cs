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
    public DbSet<Wallet>          Wallets          => Set<Wallet>();
    public DbSet<Category>        FinancialCategories => Set<Category>();
    public DbSet<PaymentMethod>   PaymentMethods   => Set<PaymentMethod>();
    public DbSet<Transaction>     Transactions     => Set<Transaction>();
    public DbSet<Reconciliation>  Reconciliations  => Set<Reconciliation>();

    // Agendamentos
    public DbSet<ProfessionalSpecialty>     ProfessionalSpecialties     => Set<ProfessionalSpecialty>();
    public DbSet<ProfessionalSpecialtyLink> ProfessionalSpecialtyLinks  => Set<ProfessionalSpecialtyLink>();
    public DbSet<Professional>              Professionals               => Set<Professional>();
    public DbSet<Patient>                   Patients                    => Set<Patient>();
    public DbSet<Service>                   Services                    => Set<Service>();
    public DbSet<ProfessionalService>       ProfessionalServices        => Set<ProfessionalService>();
    public DbSet<CommissionRule>            CommissionRules             => Set<CommissionRule>();
    public DbSet<ProfessionalSchedule>      ProfessionalSchedules       => Set<ProfessionalSchedule>();
    public DbSet<Appointment>               Appointments                => Set<Appointment>();
    public DbSet<AppointmentRecurrence>     AppointmentRecurrences      => Set<AppointmentRecurrence>();
    public DbSet<AppointmentTask>           AppointmentTasks            => Set<AppointmentTask>();

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

        modelBuilder.Entity<Wallet>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Category>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<PaymentMethod>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Transaction>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Reconciliation>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<ProfessionalSpecialty>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<ProfessionalSpecialtyLink>()
            .HasQueryFilter(x => x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Professional>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Patient>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Service>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<ProfessionalService>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<CommissionRule>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<ProfessionalSchedule>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<AppointmentRecurrence>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        modelBuilder.Entity<AppointmentTask>()
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

