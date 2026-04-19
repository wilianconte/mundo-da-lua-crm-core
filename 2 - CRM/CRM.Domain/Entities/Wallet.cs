using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Wallet : TenantEntity
{
    public string  Name           { get; private set; } = string.Empty;
    public decimal InitialBalance { get; private set; }
    public bool    IsActive       { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = [];

    private Wallet() { }

    public static Wallet Create(Guid tenantId, string name, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wallet name is required.", nameof(name));

        return new Wallet
        {
            TenantId       = tenantId,
            Name           = name.Trim(),
            InitialBalance = initialBalance,
            IsActive       = true,
        };
    }

    public void Update(string name, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wallet name is required.", nameof(name));

        Name           = name.Trim();
        InitialBalance = initialBalance;
        Touch();
    }

    public void SetActive()   { IsActive = true;  Touch(); }
    public void SetInactive() { IsActive = false; Touch(); }

    /// <summary>
    /// Computes the current balance from initial balance and transaction totals.
    /// Balance is NOT stored — it is calculated on demand.
    /// </summary>
    public decimal RecalculateBalance(decimal incomeTotal, decimal expenseTotal) =>
        InitialBalance + incomeTotal - expenseTotal;
}
