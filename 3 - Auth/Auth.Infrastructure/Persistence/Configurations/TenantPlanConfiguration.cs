using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Infrastructure.Persistence.Configurations;

public sealed class TenantPlanConfiguration : IEntityTypeConfiguration<TenantPlan>
{
    public void Configure(EntityTypeBuilder<TenantPlan> builder)
    {
        builder.ToTable("tenant_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.PlanId).IsRequired();

        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate);
        builder.Property(x => x.IsTrial).IsRequired();
        builder.Property(x => x.FallbackPlanId);
        builder.Property(x => x.CancelledAt);
        builder.Property(x => x.PausedAt);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FallbackPlan)
            .WithMany()
            .HasForeignKey(x => x.FallbackPlanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // UNIQUE (TenantId) WHERE Status = 0 (Active)
        builder.HasIndex(x => x.TenantId)
            .IsUnique()
            .HasFilter("\"Status\" = 0")
            .HasDatabaseName("IX_tenant_plans_tenantid_active");

        // UNIQUE (TenantId) WHERE Status = 1 (Paused)
        builder.HasIndex(x => x.TenantId)
            .IsUnique()
            .HasFilter("\"Status\" = 1")
            .HasDatabaseName("IX_tenant_plans_tenantid_paused");

        // UNIQUE (TenantId, PlanId) WHERE IsTrial = true
        builder.HasIndex(x => new { x.TenantId, x.PlanId })
            .IsUnique()
            .HasFilter("\"IsTrial\" = true")
            .HasDatabaseName("IX_tenant_plans_trial_unique");
    }
}
