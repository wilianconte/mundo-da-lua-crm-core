using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class AppointmentTaskConfiguration : IEntityTypeConfiguration<AppointmentTask>
{
    public void Configure(EntityTypeBuilder<AppointmentTask> builder)
    {
        builder.ToTable("appointment_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.AppointmentId).IsRequired();
        builder.Property(x => x.Type).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.AssignedToRole).HasMaxLength(100);
        builder.Property(x => x.Result).HasMaxLength(200);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AppointmentId, x.Status });
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => x.IsDeleted);
    }
}
