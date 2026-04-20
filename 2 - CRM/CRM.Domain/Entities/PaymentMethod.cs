using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class PaymentMethod : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid WalletId { get; private set; }

    // EF Core navigation — do not use in domain logic
    public Wallet? Wallet { get; private set; }

    private PaymentMethod() { }

    public static PaymentMethod Create(Guid tenantId, string name, Guid walletId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("PaymentMethod name is required.", nameof(name));
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId is required.", nameof(walletId));

        return new PaymentMethod { TenantId = tenantId, Name = name.Trim(), WalletId = walletId };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("PaymentMethod name is required.", nameof(name));

        Name = name.Trim();
        Touch();
    }
}
