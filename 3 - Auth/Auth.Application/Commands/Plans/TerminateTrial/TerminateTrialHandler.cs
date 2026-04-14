using MediatR;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.TerminateTrial;

public sealed class TerminateTrialHandler : IRequestHandler<TerminateTrialCommand, Result>
{
    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IBillingRepository    _billingRepository;
    private readonly IPlanRepository       _planRepository;

    public TerminateTrialHandler(
        ITenantPlanRepository tenantPlanRepository,
        IBillingRepository billingRepository,
        IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _billingRepository    = billingRepository;
        _planRepository       = planRepository;
    }

    public async Task<Result> Handle(TerminateTrialCommand request, CancellationToken ct)
    {
        // 1. Busca TenantPlan Active
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(request.TenantId, ct);
        if (activePlan is null)
            return Result.Failure("TENANT_PLAN_NOT_FOUND", "Nenhum plano ativo encontrado para o tenant.");

        // 2. Valida IsTrial = true
        if (!activePlan.IsTrial)
            return Result.Failure("PLAN_NOT_TRIAL",
                "O plano ativo não é um plano de trial.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 3. TenantPlan atual: Status = Expired, EndDate = hoje
        activePlan.Expire(today);
        _tenantPlanRepository.Update(activePlan);

        // 4. Verifica TenantPlan Paused para o tenant
        var pausedPlan = await _tenantPlanRepository.GetPausedByTenantIdAsync(request.TenantId, ct);

        if (pausedPlan is not null)
        {
            // SE EXISTE plano pausado: retoma o plano pausado com os dias restantes
            var remainingDays = pausedPlan.EndDate.HasValue && pausedPlan.PausedAt.HasValue
                ? (int)(pausedPlan.EndDate.Value.DayNumber - pausedPlan.PausedAt.Value.DayNumber)
                : 30;

            var newEndDate = today.AddDays(Math.Max(remainingDays, 1));
            pausedPlan.Resume(newEndDate);
            _tenantPlanRepository.Update(pausedPlan);
        }
        else
        {
            // SE NÃO EXISTE plano pausado: DowngradeToPlanId é obrigatório
            if (request.DowngradeToPlanId is null)
                return Result.Failure("DOWNGRADE_PLAN_REQUIRED",
                    "Informe o plano de destino após o trial (não há plano pausado para retomar).");

            var downgradePlan = await _planRepository.GetByIdAsync(request.DowngradeToPlanId.Value, ct);
            if (downgradePlan is null || !downgradePlan.IsActive)
                return Result.Failure("PLAN_NOT_FOUND", "Plano de destino não encontrado ou inativo.");

            var freePlan = await _planRepository.GetFreePlanAsync(ct);

            if (downgradePlan.Price == 0)
            {
                // Downgrade para Free: sem EndDate, sem FallbackPlanId, sem Billing
                var freeTenantPlan = TenantPlan.Create(
                    tenantId:       request.TenantId,
                    planId:         downgradePlan.Id,
                    startDate:      today,
                    endDate:        null,
                    isTrial:        false,
                    fallbackPlanId: null,
                    status:         TenantPlanStatus.Active);

                await _tenantPlanRepository.AddAsync(freeTenantPlan, ct);
            }
            else
            {
                // Downgrade para plano pago: EndDate = hoje + 1 mês, Billing (valor cheio)
                var paidTenantPlan = TenantPlan.Create(
                    tenantId:       request.TenantId,
                    planId:         downgradePlan.Id,
                    startDate:      today,
                    endDate:        today.AddMonths(1),
                    isTrial:        false,
                    fallbackPlanId: freePlan?.Id,
                    status:         TenantPlanStatus.Active);

                await _tenantPlanRepository.AddAsync(paidTenantPlan, ct);

                var billing = Billing.Create(
                    tenantId:       request.TenantId,
                    tenantPlanId:   paidTenantPlan.Id,
                    amount:         downgradePlan.Price,
                    dueDate:        today.AddDays(10),
                    referenceMonth: today.ToString("yyyy-MM"));

                await _billingRepository.AddAsync(billing, ct);
            }
        }

        await _tenantPlanRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
