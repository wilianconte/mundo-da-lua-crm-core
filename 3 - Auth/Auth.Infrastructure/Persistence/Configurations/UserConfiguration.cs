using MyCRM.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyCRM.Auth.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsAdmin).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.PersonId);

        builder.Property(x => x.PasswordResetToken).HasMaxLength(128);
        builder.Property(x => x.PasswordResetTokenExpiresAt);
        builder.HasIndex(x => x.PasswordResetToken).IsUnique().HasFilter("\"PasswordResetToken\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => x.IsDeleted);

        // Uma Person pode estar vinculada a no máximo um User por tenant
        builder.HasIndex(x => new { x.TenantId, x.PersonId })
            .IsUnique()
            .HasFilter("\"PersonId\" IS NOT NULL");
    }
}
