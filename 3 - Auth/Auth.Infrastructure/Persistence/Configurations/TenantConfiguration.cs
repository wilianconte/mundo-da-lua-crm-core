using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.CompanyId).IsRequired();
        builder.Property(x => x.OwnerPersonId);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        // Sem query filter por TenantId — Tenant é a raiz do tenant, não possui TenantId próprio.
        // Filtro apenas para soft-delete.
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.Name);
    }
}
