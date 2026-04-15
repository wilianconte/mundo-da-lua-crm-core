using MediatR;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.UpgradeTenantPlan;

public sealed class UpgradeTenantPlanHandler : IRequestHandler<UpgradeTenantPlanCommand, Result>
{
    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IBillingRepository    _billingRepository;
    private readonly IPlanRepository       _planRepository;

    public UpgradeTenantPlanHandler(
        ITenantPlanRepository tenantPlanRepository,
        IBillingRepository billingRepository,
        IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _billingRepository    = billingRepository;
        _planRepository       = planRepository;
    }

    public async Task<Result> Handle(UpgradeTenantPlanCommand request, CancellationToken ct)
    {
        // 1. Busca TenantPlan Active do tenant
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(request.TenantId, ct);
        if (activePlan is null)
            return Result.Failure("TENANT_PLAN_NOT_FOUND", "Nenhum plano ativo encontrado para o tenant.");

        // 2. Busca o novo plano — erro se não existir ou não ativo
        var newPlan = await _planRepository.GetByIdAsync(request.NewPlanId, ct);
        if (newPlan is null || !newPlan.IsActive)
            return Result.Failure("PLAN_NOT_FOUND", "Plano não encontrado ou inativo.");

        // 3. Valida que o destino não é Free (downgrade para Free usa CancelTenantPlan — RN-028.3)
        if (newPlan.Price == 0)
            return Result.Failure("UPGRADE_TO_FREE_NOT_ALLOWED",
                "Para migrar para o plano gratuito, utilize o cancelamento com plano de destino Free.");

        // 4. Valida que o novo plano é diferente do atual
        if (activePlan.PlanId == request.NewPlanId)
            return Result.Failure("PLAN_SAME_AS_CURRENT", "O tenant já está neste plano.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 5. Se TenantPlan atual IsTrial = false: cancela todos os Billings Pending do TenantPlan atual
        if (!activePlan.IsTrial)
        {
            var pendingBillings = await _billingRepository.GetAllPendingByTenantPlanIdAsync(activePlan.Id, ct);
            foreach (var b in pendingBillings)
            {
                b.Cancel();
                _billingRepository.Update(b);
            }
        }

        // Calcula valor do Billing para o novo plano
        decimal billingAmount;
        if (activePlan.IsTrial)
        {
            // Origem trial: valor cheio do novo plano
            billingAmount = newPlan.Price;
        }
        else
        {
            // Origem pago: valor proporcional aos dias restantes
            var diasRestantes = activePlan.EndDate.HasValue
                ? activePlan.EndDate.Value.DayNumber - today.DayNumber
                : 0;
            var diasTotais = activePlan.EndDate.HasValue
                ? activePlan.EndDate.Value.DayNumber - activePlan.StartDate.DayNumber
                : 0;

            billingAmount = diasTotais > 0 && diasRestantes > 0
                ? Math.Round(newPlan.Price * diasRestantes / diasTotais, 2)
                : newPlan.Price;
        }

        // 6. TenantPlan atual: Status = Upgraded, EndDate = hoje
        activePlan.Upgrade(today);
        _tenantPlanRepository.Update(activePlan);

        // 7. Se existir TenantPlan Paused para o tenant: Status = Cancelled
        var pausedPlan = await _tenantPlanRepository.GetPausedByTenantIdAsync(request.TenantId, ct);
        if (pausedPlan is not null)
        {
            pausedPlan.Cancel(today);
            _tenantPlanRepository.Update(pausedPlan);
        }

        // Busca o plano Free para FallbackPlanId
        var freePlan = await _planRepository.GetFreePlanAsync(ct);

        // 8. Cria novo TenantPlan: pago, 1 mês, fallback = Free
        var newTenantPlan = TenantPlan.Create(
            tenantId:       request.TenantId,
            planId:         request.NewPlanId,
            startDate:      today,
            endDate:        today.AddMonths(1),
            isTrial:        false,
            fallbackPlanId: freePlan?.Id,
            status:         TenantPlanStatus.Active);

        await _tenantPlanRepository.AddAsync(newTenantPlan, ct);

        // 9. Gera Billing (sempre pago aqui, pois Free foi rejeitado acima)
        if (newPlan.Price > 0)
        {
            var billing = Billing.Create(
                tenantId:       request.TenantId,
                tenantPlanId:   newTenantPlan.Id,
                amount:         billingAmount,
                dueDate:        today.AddDays(10),
                referenceMonth: today.ToString("yyyy-MM"));

            await _billingRepository.AddAsync(billing, ct);
        }

        // 10. Persiste tudo atomicamente
        await _tenantPlanRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
