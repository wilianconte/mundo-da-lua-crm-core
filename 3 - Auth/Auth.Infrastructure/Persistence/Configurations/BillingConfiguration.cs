using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Infrastructure.Persistence.Configurations;

public sealed class BillingConfiguration : IEntityTypeConfiguration<Billing>
{
    public void Configure(EntityTypeBuilder<Billing> builder)
    {
        builder.ToTable("billings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.TenantPlanId).IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("numeric(10,2)");

        builder.Property(x => x.DueDate).IsRequired();
        builder.Property(x => x.PaidAt);

        builder.Property(x => x.ReferenceMonth)
            .IsRequired()
            .HasMaxLength(7); // YYYY-MM

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.InvoiceUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.TenantPlan)
            .WithMany()
            .HasForeignKey(x => x.TenantPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // UNIQUE (TenantId, ReferenceMonth) WHERE Status = 0 (Pending)
        builder.HasIndex(x => new { x.TenantId, x.ReferenceMonth })
            .IsUnique()
            .HasFilter("\"Status\" = 0")
            .HasDatabaseName("IX_billings_tenant_month_pending");
    }
}
