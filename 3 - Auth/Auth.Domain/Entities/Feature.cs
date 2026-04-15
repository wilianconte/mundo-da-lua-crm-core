using MyCRM.Auth.Domain.Enums;
using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Feature/capacidade que um plano pode ter (ex: max_students, has_reports).
/// Global — não é tenant-specific.
/// </summary>
public sealed class Feature : BaseEntity
{
    public string Key { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public FeatureType Type { get; private set; }

    public ICollection<PlanFeature> PlanFeatures { get; private set; } = [];

    private Feature() { }

    public static Feature Create(string key, string description, FeatureType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Feature
        {
            Key         = key.Trim().ToLowerInvariant(),
            Description = description.Trim(),
            Type        = type,
        };
    }
}
