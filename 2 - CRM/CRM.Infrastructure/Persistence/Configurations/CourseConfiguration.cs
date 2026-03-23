using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Identity
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.Type).IsRequired().HasConversion<int>();
        builder.Property(x => x.Description).HasMaxLength(2000);

        // Schedule
        builder.Property(x => x.ScheduleDescription).HasMaxLength(500);

        // Capacity & Workload
        builder.Property(x => x.Capacity);
        builder.Property(x => x.Workload);

        // Status & Flags
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique: course code per tenant (optional field)
        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.Type });
        builder.HasIndex(x => x.IsDeleted);
    }
}
