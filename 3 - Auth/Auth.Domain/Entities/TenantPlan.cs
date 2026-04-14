using MyCRM.Auth.Domain.Enums;
using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Assinatura ativa (ou histórica) de um tenant a um plano da plataforma.
/// Um tenant deve ter exatamente um TenantPlan com Status = Active a qualquer momento.
///
/// Herda de BaseEntity (não TenantEntity) — TenantId é explícito porque é uma entidade
/// de plataforma cruzada, não filtrada pelo query filter de tenant.
/// </summary>
public sealed class TenantPlan : BaseEntity
{
    public Guid TenantId { get; private set; }

    public Guid PlanId { get; private set; }
    public Plan Plan { get; private set; } = default!;

    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public bool IsTrial { get; private set; }

    /// <summary>Plano de fallback ao término/cancelamento. Normalmente o plano Free.</summary>
    public Guid? FallbackPlanId { get; private set; }
    public Plan? FallbackPlan { get; private set; }

    public DateOnly? CancelledAt { get; private set; }

    /// <summary>Data em que o plano foi pausado para trial de outro plano.</summary>
    public DateOnly? PausedAt { get; private set; }

    public TenantPlanStatus Status { get; private set; }

    private TenantPlan() { }

    public static TenantPlan Create(
        Guid tenantId,
        Guid planId,
        DateOnly startDate,
        DateOnly? endDate,
        bool isTrial,
        Guid? fallbackPlanId,
        TenantPlanStatus status = TenantPlanStatus.Active)
    {
        return new TenantPlan
        {
            TenantId       = tenantId,
            PlanId         = planId,
            StartDate      = startDate,
            EndDate        = endDate,
            IsTrial        = isTrial,
            FallbackPlanId = fallbackPlanId,
            Status         = status,
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void Expire(DateOnly? endDate = null)
    {
        Status  = TenantPlanStatus.Expired;
        EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        Touch();
    }

    public void Upgrade(DateOnly? endDate = null)
    {
        Status  = TenantPlanStatus.Upgraded;
        EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        Touch();
    }

    public void Cancel(DateOnly cancelledAt)
    {
        Status      = TenantPlanStatus.Cancelled;
        CancelledAt = cancelledAt;
        Touch();
    }

    public void SetPendingCancellation(Guid fallbackPlanId, DateOnly cancelledAt)
    {
        Status         = TenantPlanStatus.PendingCancellation;
        FallbackPlanId = fallbackPlanId;
        CancelledAt    = cancelledAt;
        Touch();
    }

    public void RevertCancellation(Guid fallbackPlanId)
    {
        Status         = TenantPlanStatus.Active;
        CancelledAt    = null;
        FallbackPlanId = fallbackPlanId;
        Touch();
    }

    public void Pause(DateOnly pausedAt)
    {
        Status   = TenantPlanStatus.Paused;
        PausedAt = pausedAt;
        Touch();
    }

    public void Resume(DateOnly newEndDate)
    {
        Status   = TenantPlanStatus.Active;
        PausedAt = null;
        EndDate  = newEndDate;
        Touch();
    }

    public void SetEndDate(DateOnly endDate)
    {
        EndDate = endDate;
        Touch();
    }
}
