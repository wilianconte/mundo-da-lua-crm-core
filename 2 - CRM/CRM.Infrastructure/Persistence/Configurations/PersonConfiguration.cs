using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("people");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Identity
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.PreferredName).HasMaxLength(150);
        builder.Property(x => x.DocumentNumber).HasMaxLength(30);
        builder.Property(x => x.BirthDate);
        builder.Property(x => x.Gender).HasConversion<int>();
        builder.Property(x => x.MaritalStatus).HasConversion<int>();
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.Occupation).HasMaxLength(200);

        // Contact
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.PrimaryPhone).HasMaxLength(30);
        builder.Property(x => x.SecondaryPhone).HasMaxLength(30);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(30);

        // Profile
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(2000);

        // Status & Notes
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique document number per tenant — partial index ignores NULLs
        builder.HasIndex(x => new { x.TenantId, x.DocumentNumber })
            .IsUnique()
            .HasFilter("document_number IS NOT NULL");

        // Unique email per tenant — partial index ignores NULLs
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique()
            .HasFilter("email IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.IsDeleted);
    }
}
