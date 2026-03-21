using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Domain.Entities;

public class Customer : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Document { get; private set; }
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public Address? Address { get; private set; }
    public string? Notes { get; private set; }

    private Customer() { }

    public static Customer Create(
        Guid tenantId,
        string name,
        string email,
        CustomerType type,
        string? phone = null,
        string? document = null)
    {
        return new Customer
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Type = type,
            Phone = phone?.Trim(),
            Document = document?.Trim(),
            Status = CustomerStatus.Active,
        };
    }

    public void Update(
        string name,
        string email,
        string? phone,
        string? document,
        string? notes)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        Document = document?.Trim();
        Notes = notes?.Trim();
        Touch();
    }

    public void SetAddress(Address address)
    {
        Address = address;
        Touch();
    }

    public void Deactivate()
    {
        Status = CustomerStatus.Inactive;
        Touch();
    }

    public void Activate()
    {
        Status = CustomerStatus.Active;
        Touch();
    }
}
