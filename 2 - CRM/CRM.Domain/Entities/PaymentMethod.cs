using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class PaymentMethod : TenantEntity
{
    public string Name { get; private set; } = string.Empty;

    private PaymentMethod() { }

    public static PaymentMethod Create(Guid tenantId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("PaymentMethod name is required.", nameof(name));

        return new PaymentMethod { TenantId = tenantId, Name = name.Trim() };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("PaymentMethod name is required.", nameof(name));

        Name = name.Trim();
        Touch();
    }
}
