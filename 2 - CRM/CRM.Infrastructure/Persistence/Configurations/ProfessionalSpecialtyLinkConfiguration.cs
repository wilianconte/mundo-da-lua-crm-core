using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalSpecialtyLinkConfiguration : IEntityTypeConfiguration<ProfessionalSpecialtyLink>
{
    public void Configure(EntityTypeBuilder<ProfessionalSpecialtyLink> builder)
    {
        builder.ToTable("professional_specialty_links");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.ProfessionalId).IsRequired();
        builder.Property(x => x.SpecialtyId).IsRequired();

        builder.HasOne(x => x.Professional)
            .WithMany()
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Specialty)
            .WithMany()
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ProfessionalId, x.SpecialtyId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
