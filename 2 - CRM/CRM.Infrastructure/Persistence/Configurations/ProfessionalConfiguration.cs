using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        builder.Property(x => x.PersonId).IsRequired();
        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status).IsRequired()
            .HasConversion<string>().HasMaxLength(50);

        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.LicenseNumber).HasMaxLength(100);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);

        builder.HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        // Only one active Professional per Person per tenant (RN-042)
        builder.HasIndex(x => new { x.TenantId, x.PersonId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeleted);
    }
}
