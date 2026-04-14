using MediatR;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.CancelTenantPlan;

public sealed class CancelTenantPlanHandler : IRequestHandler<CancelTenantPlanCommand, Result>
{
    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IBillingRepository    _billingRepository;
    private readonly IPlanRepository       _planRepository;

    public CancelTenantPlanHandler(
        ITenantPlanRepository tenantPlanRepository,
        IBillingRepository billingRepository,
        IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _billingRepository    = billingRepository;
        _planRepository       = planRepository;
    }

    public async Task<Result> Handle(CancelTenantPlanCommand request, CancellationToken ct)
    {
        // 1. Busca TenantPlan Active
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(request.TenantId, ct);
        if (activePlan is null)
            return Result.Failure("TENANT_PLAN_NOT_FOUND", "Nenhum plano ativo encontrado para o tenant.");

        // 2. Valida que IsTrial = false (trial termina via TerminateTrial)
        if (activePlan.IsTrial)
            return Result.Failure("PLAN_CANCEL_TRIAL_NOT_ALLOWED",
                "Planos em período de trial devem ser encerrados via TerminateTrial.");

        // 3. Valida Status = Active
        if (activePlan.Status != TenantPlanStatus.Active)
            return Result.Failure("PLAN_NOT_ACTIVE",
                "O plano não está ativo e não pode ser cancelado.");

        // Valida que o plano de downgrade existe
        var downgradePlan = await _planRepository.GetByIdAsync(request.DowngradeToPlanId, ct);
        if (downgradePlan is null || !downgradePlan.IsActive)
            return Result.Failure("DOWNGRADE_PLAN_NOT_FOUND", "Plano de downgrade não encontrado ou inativo.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 4. TenantPlan: FallbackPlanId = DowngradeToPlanId, CancelledAt = hoje, Status = PendingCancellation
        activePlan.SetPendingCancellation(request.DowngradeToPlanId, today);
        _tenantPlanRepository.Update(activePlan);

        // 5. Cancela todos os Billings Pending do TenantPlan
        var pendingBillings = await _billingRepository.GetAllPendingByTenantPlanIdAsync(activePlan.Id, ct);
        foreach (var b in pendingBillings)
        {
            b.Cancel();
            _billingRepository.Update(b);
        }

        await _tenantPlanRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
