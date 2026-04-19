using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Service : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal DefaultPrice { get; private set; }
    public int DefaultDurationInMinutes { get; private set; }
    public bool IsActive { get; private set; }

    private Service() { }

    public static Service Create(Guid tenantId, string name, decimal defaultPrice, int defaultDurationInMinutes, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));
        if (defaultPrice <= 0)
            throw new ArgumentException("DefaultPrice must be greater than zero.", nameof(defaultPrice));
        if (defaultDurationInMinutes <= 0)
            throw new ArgumentException("DefaultDurationInMinutes must be greater than zero.", nameof(defaultDurationInMinutes));

        return new Service
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DefaultPrice = defaultPrice,
            DefaultDurationInMinutes = defaultDurationInMinutes,
            IsActive = true
        };
    }

    public void Update(string name, decimal defaultPrice, int defaultDurationInMinutes, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));
        if (defaultPrice <= 0)
            throw new ArgumentException("DefaultPrice must be greater than zero.", nameof(defaultPrice));
        if (defaultDurationInMinutes <= 0)
            throw new ArgumentException("DefaultDurationInMinutes must be greater than zero.", nameof(defaultDurationInMinutes));

        Name = name.Trim();
        Description = description?.Trim();
        DefaultPrice = defaultPrice;
        DefaultDurationInMinutes = defaultDurationInMinutes;
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
}
