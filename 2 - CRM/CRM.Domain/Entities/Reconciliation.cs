using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

public sealed class Reconciliation : TenantEntity
{
    public Guid     WalletId       { get; private set; }
    public Guid     TransactionId  { get; private set; }
    public string   ExternalId     { get; private set; } = string.Empty;
    public decimal  ExternalAmount { get; private set; }
    public DateTime ExternalDate   { get; private set; }
    public DateTime MatchedAt      { get; private set; }

    public Transaction? Transaction { get; private set; }

    private Reconciliation() { }

    public static Reconciliation Create(
        Guid tenantId,
        Guid walletId,
        Guid transactionId,
        string externalId,
        decimal externalAmount,
        DateTime externalDate)
    {
        return new Reconciliation
        {
            TenantId       = tenantId,
            WalletId       = walletId,
            TransactionId  = transactionId,
            ExternalId     = externalId.Trim(),
            ExternalAmount = externalAmount,
            ExternalDate   = externalDate,
            MatchedAt      = DateTime.UtcNow,
        };
    }

    public new void SoftDelete() => base.SoftDelete();
}
