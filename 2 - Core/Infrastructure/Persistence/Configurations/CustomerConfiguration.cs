using MyCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Document).HasMaxLength(30);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasMaxLength(300).HasColumnName("address_street");
            a.Property(p => p.Number).HasMaxLength(20).HasColumnName("address_number");
            a.Property(p => p.Complement).HasMaxLength(100).HasColumnName("address_complement");
            a.Property(p => p.Neighborhood).HasMaxLength(150).HasColumnName("address_neighborhood");
            a.Property(p => p.City).HasMaxLength(150).HasColumnName("address_city");
            a.Property(p => p.State).HasMaxLength(2).HasColumnName("address_state");
            a.Property(p => p.ZipCode).HasMaxLength(10).HasColumnName("address_zip_code");
            a.Property(p => p.Country).HasMaxLength(2).HasColumnName("address_country").HasDefaultValue("BR");
        });

        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.IsDeleted);
    }
}
