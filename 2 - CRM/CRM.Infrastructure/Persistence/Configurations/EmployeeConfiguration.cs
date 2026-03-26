using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Person reference
        builder.Property(x => x.PersonId).IsRequired();
        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing manager relationship
        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Employment fields
        builder.Property(x => x.EmployeeCode).HasMaxLength(50);
        builder.Property(x => x.Position).HasMaxLength(200);
        builder.Property(x => x.Department).HasMaxLength(200);

        // Contract & schedule fields
        builder.Property(x => x.ContractType).HasMaxLength(100);
        builder.Property(x => x.WorkSchedule).HasMaxLength(200);
        builder.Property(x => x.WorkloadHours).HasColumnType("numeric(5,2)");

        // Payroll & org fields
        builder.Property(x => x.PayrollNumber).HasMaxLength(50);
        builder.Property(x => x.CostCenter).HasMaxLength(100);

        // Status & Notes
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique: one active employee record per person per tenant
        builder.HasIndex(x => new { x.TenantId, x.PersonId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Unique: employee code per tenant (optional field)
        builder.HasIndex(x => new { x.TenantId, x.EmployeeCode })
            .IsUnique()
            .HasFilter("\"EmployeeCode\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.IsDeleted);
    }
}
