using MyCRM.Auth.Domain.Enums;
using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Cobrança gerada quando um tenant faz upgrade para um plano pago.
/// Herda de BaseEntity — TenantId é explícito.
/// </summary>
public sealed class Billing : BaseEntity
{
    public Guid TenantId { get; private set; }

    public Guid TenantPlanId { get; private set; }
    public TenantPlan TenantPlan { get; private set; } = default!;

    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }

    /// <summary>Mês de referência da cobrança no formato YYYY-MM.</summary>
    public string ReferenceMonth { get; private set; } = default!;

    public BillingStatus Status { get; private set; }
    public string? InvoiceUrl { get; private set; }

    private Billing() { }

    public static Billing Create(
        Guid tenantId,
        Guid tenantPlanId,
        decimal amount,
        DateOnly dueDate,
        string referenceMonth,
        BillingStatus status = BillingStatus.Pending,
        string? invoiceUrl = null)
    {
        return new Billing
        {
            TenantId       = tenantId,
            TenantPlanId   = tenantPlanId,
            Amount         = amount,
            DueDate        = dueDate,
            ReferenceMonth = referenceMonth,
            Status         = status,
            InvoiceUrl     = invoiceUrl,
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void MarkAsPaid(DateTime paidAt)
    {
        Status = BillingStatus.Paid;
        PaidAt = paidAt;
        Touch();
    }

    public void Cancel()
    {
        Status = BillingStatus.Cancelled;
        Touch();
    }

    public void MarkAsOverdue()
    {
        Status = BillingStatus.Overdue;
        Touch();
    }
}
