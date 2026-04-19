using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class AppointmentRecurrenceConfiguration : IEntityTypeConfiguration<AppointmentRecurrence>
{
    public void Configure(EntityTypeBuilder<AppointmentRecurrence> builder)
    {
        builder.ToTable("appointment_recurrences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Frequency).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ParentAppointmentId).IsRequired();
        builder.Property(x => x.CreatedOccurrences).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.ParentAppointmentId);
        builder.HasIndex(x => x.IsDeleted);
    }
}
