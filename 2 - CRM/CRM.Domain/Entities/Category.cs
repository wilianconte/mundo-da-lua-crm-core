using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Category : TenantEntity
{
    public string Name { get; private set; } = string.Empty;

    private Category() { }

    public static Category Create(Guid tenantId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        return new Category { TenantId = tenantId, Name = name.Trim() };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name.Trim();
        Touch();
    }
}
