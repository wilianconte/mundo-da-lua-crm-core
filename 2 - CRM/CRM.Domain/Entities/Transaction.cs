using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Transaction : TenantEntity
{
    public Guid            WalletId         { get; private set; }
    public TransactionType Type             { get; private set; }
    public decimal         Amount           { get; private set; }
    public string          Description      { get; private set; } = string.Empty;
    public Guid            CategoryId       { get; private set; }
    public Guid            PaymentMethodId  { get; private set; }
    public DateTime        TransactionDate  { get; private set; }
    public bool            IsReconciled     { get; private set; }

    public Wallet?        Wallet        { get; private set; }
    public Category?      Category      { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        Guid tenantId,
        Guid walletId,
        TransactionType type,
        decimal amount,
        string description,
        Guid categoryId,
        Guid paymentMethodId,
        DateTime transactionDate)
    {
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(amount));

        return new Transaction
        {
            TenantId        = tenantId,
            WalletId        = walletId,
            Type            = type,
            Amount          = amount,
            Description     = description.Trim(),
            CategoryId      = categoryId,
            PaymentMethodId = paymentMethodId,
            TransactionDate = transactionDate,
            IsReconciled    = false,
        };
    }

    public void Update(
        decimal amount,
        string description,
        Guid categoryId,
        Guid paymentMethodId,
        DateTime transactionDate)
    {
        if (IsReconciled)
            throw new InvalidOperationException("Cannot edit a reconciled transaction.");

        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(amount));

        Amount          = amount;
        Description     = description.Trim();
        CategoryId      = categoryId;
        PaymentMethodId = paymentMethodId;
        TransactionDate = transactionDate;
        Touch();
    }

    public void MarkAsReconciled()
    {
        IsReconciled = true;
        Touch();
    }

    public new void SoftDelete()
    {
        if (IsReconciled)
            throw new InvalidOperationException("Cannot delete a reconciled transaction.");

        base.SoftDelete();
    }
}
