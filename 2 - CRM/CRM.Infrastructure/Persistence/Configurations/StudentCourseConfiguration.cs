using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        builder.ToTable("student_courses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        // Student reference
        builder.Property(x => x.StudentId).IsRequired();
        builder.HasOne(x => x.Student)
            .WithMany(s => s.Courses)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course reference
        builder.Property(x => x.CourseId).IsRequired();
        builder.HasOne(x => x.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrollment lifecycle
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.ClassGroup).HasMaxLength(100);
        builder.Property(x => x.Shift).HasMaxLength(100);
        builder.Property(x => x.ScheduleDescription).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Audit
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // Unique: prevent simultaneous active/pending duplicate enrollments for the same student+course
        // Historical re-enrollment (after cancellation/completion) is allowed — only IsDeleted=false is filtered.
        // Business logic further restricts based on Status = Active/Pending.
        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.CourseId })
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => new { x.TenantId, x.StudentId });
        builder.HasIndex(x => new { x.TenantId, x.CourseId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.IsDeleted);
    }
}
