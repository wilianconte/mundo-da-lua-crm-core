using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Person reference
        builder.Property(x => x.PersonId).IsRequired();
        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrollment fields
        builder.Property(x => x.RegistrationNumber).HasMaxLength(50);
        builder.Property(x => x.SchoolName).HasMaxLength(200);
        builder.Property(x => x.GradeOrClass).HasMaxLength(100);
        builder.Property(x => x.EnrollmentType).HasMaxLength(100);
        builder.Property(x => x.ClassGroup).HasMaxLength(50);

        // Status & Notes
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.AcademicObservation).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique: one active student record per person per tenant
        builder.HasIndex(x => new { x.TenantId, x.PersonId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Unique: registration number per tenant (optional field)
        builder.HasIndex(x => new { x.TenantId, x.RegistrationNumber })
            .IsUnique()
            .HasFilter("\"RegistrationNumber\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.IsDeleted);
    }
}
