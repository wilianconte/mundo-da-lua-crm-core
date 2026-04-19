using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class ProfessionalSpecialty : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ProfessionalSpecialty() { }

    public static ProfessionalSpecialty Create(Guid tenantId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Specialty name is required.", nameof(name));

        return new ProfessionalSpecialty
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim()
        };
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Specialty name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Touch();
    }
}
