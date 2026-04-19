using MyCRM.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.CRM.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.ProfessionalId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.ServiceId).IsRequired();
        builder.Property(x => x.StartDateTime).IsRequired();
        builder.Property(x => x.EndDateTime).IsRequired();
        builder.Property(x => x.Type).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Price).IsRequired().HasPrecision(10, 2);
        builder.Property(x => x.PaymentReceiver).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.PaymentMethodId).IsRequired();
        builder.Property(x => x.Status).IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.MeetingLink).HasMaxLength(500);
        builder.Property(x => x.ConfirmedBy).HasMaxLength(100);
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        // Address as owned type (embedded columns)
        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(x => x.Street).HasMaxLength(300).HasColumnName("address_street");
            a.Property(x => x.Number).HasMaxLength(20).HasColumnName("address_number");
            a.Property(x => x.Complement).HasMaxLength(100).HasColumnName("address_complement");
            a.Property(x => x.Neighborhood).HasMaxLength(200).HasColumnName("address_neighborhood");
            a.Property(x => x.City).HasMaxLength(200).HasColumnName("address_city");
            a.Property(x => x.State).HasMaxLength(2).HasColumnName("address_state");
            a.Property(x => x.ZipCode).HasMaxLength(20).HasColumnName("address_zip_code");
            a.Property(x => x.Country).HasMaxLength(10).HasColumnName("address_country");
        });

        builder.HasOne(x => x.Professional)
            .WithMany()
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Recurrence)
            .WithMany()
            .HasForeignKey(x => x.RecurrenceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.TenantId, x.ProfessionalId, x.StartDateTime });
        builder.HasIndex(x => new { x.TenantId, x.PatientId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeleted);
    }
}
