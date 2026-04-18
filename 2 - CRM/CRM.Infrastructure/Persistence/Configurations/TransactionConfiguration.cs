using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("financial_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.WalletId).IsRequired();
        builder.Property(x => x.Type).IsRequired().HasConversion<int>();
        builder.Property(x => x.Amount).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CategoryId).IsRequired();
        builder.Property(x => x.PaymentMethodId).IsRequired();
        builder.Property(x => x.TransactionDate).IsRequired();
        builder.Property(x => x.IsReconciled).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.WalletId });
        builder.HasIndex(x => new { x.TenantId, x.TransactionDate });
    }
}
