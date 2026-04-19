using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class ReconciliationConfiguration : IEntityTypeConfiguration<Reconciliation>
{
    public void Configure(EntityTypeBuilder<Reconciliation> builder)
    {
        builder.ToTable("reconciliations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.WalletId).IsRequired();
        builder.Property(x => x.TransactionId).IsRequired();
        builder.Property(x => x.ExternalId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ExternalAmount).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.ExternalDate).IsRequired();
        builder.Property(x => x.MatchedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Transaction)
            .WithMany()
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.TransactionId }).IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
