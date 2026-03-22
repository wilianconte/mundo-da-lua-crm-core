using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class StudentGuardianConfiguration : IEntityTypeConfiguration<StudentGuardian>
{
    public void Configure(EntityTypeBuilder<StudentGuardian> builder)
    {
        builder.ToTable("student_guardians");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Student reference
        builder.Property(x => x.StudentId).IsRequired();
        builder.HasOne(x => x.Student)
            .WithMany(s => s.Guardians)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Guardian person reference
        builder.Property(x => x.GuardianPersonId).IsRequired();
        builder.HasOne(x => x.GuardianPerson)
            .WithMany()
            .HasForeignKey(x => x.GuardianPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship attributes
        builder.Property(x => x.RelationshipType).IsRequired().HasConversion<int>();
        builder.Property(x => x.IsPrimaryGuardian).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsFinancialResponsible).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ReceivesNotifications).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CanPickupChild).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique: same guardian person cannot be added twice for the same student
        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.GuardianPersonId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => new { x.TenantId, x.StudentId });
        builder.HasIndex(x => x.IsDeleted);
    }
}
