using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Identity
        builder.Property(x => x.LegalName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.TradeName).HasMaxLength(300);
        builder.Property(x => x.RegistrationNumber).HasMaxLength(30);
        builder.Property(x => x.StateRegistration).HasMaxLength(30);
        builder.Property(x => x.MunicipalRegistration).HasMaxLength(30);

        // Contact
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.PrimaryPhone).HasMaxLength(30);
        builder.Property(x => x.SecondaryPhone).HasMaxLength(30);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(30);
        builder.Property(x => x.Website).HasMaxLength(500);

        // Contact Person
        builder.Property(x => x.ContactPersonName).HasMaxLength(300);
        builder.Property(x => x.ContactPersonEmail).HasMaxLength(254);
        builder.Property(x => x.ContactPersonPhone).HasMaxLength(30);

        // Classification
        builder.Property(x => x.CompanyType).HasConversion<int>();
        builder.Property(x => x.Industry).HasMaxLength(150);

        // Profile
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(2000);

        // Address — owned value object (same pattern as Customer)
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

        // Status & Notes
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique registration number per tenant — partial index ignores NULLs
        builder.HasIndex(x => new { x.TenantId, x.RegistrationNumber })
            .IsUnique()
            .HasFilter("\"RegistrationNumber\" IS NOT NULL");

        // Unique email per tenant — partial index ignores NULLs
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.IsDeleted);
    }
}
