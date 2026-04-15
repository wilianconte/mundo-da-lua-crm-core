using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Plano comercial global da plataforma (Free, Basic, Pro, etc.).
/// Não é tenant-specific — todos os tenants enxergam os mesmos planos.
/// </summary>
public sealed class Plan : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    public ICollection<PlanFeature> PlanFeatures { get; private set; } = [];

    private Plan() { }

    public static Plan Create(string name, string displayName, decimal price, bool isActive = true, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new Plan
        {
            Name        = name.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            Price       = price,
            IsActive    = isActive,
            SortOrder   = sortOrder,
        };
    }
}
