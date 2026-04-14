using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Infrastructure.Persistence.Configurations;

public sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("plan_features");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.FeatureId).IsRequired();
        builder.Property(x => x.Value);

        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.PlanFeatures)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Feature)
            .WithMany(x => x.PlanFeatures)
            .HasForeignKey(x => x.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PlanId, x.FeatureId }).IsUnique();
    }
}
