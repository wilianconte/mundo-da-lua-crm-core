using MediatR;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.StartTrial;

public sealed class StartTrialHandler : IRequestHandler<StartTrialCommand, Result>
{
    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IPlanRepository       _planRepository;

    public StartTrialHandler(
        ITenantPlanRepository tenantPlanRepository,
        IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _planRepository       = planRepository;
    }

    public async Task<Result> Handle(StartTrialCommand request, CancellationToken ct)
    {
        // 1. Busca TenantPlan Active
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(request.TenantId, ct);
        if (activePlan is null)
            return Result.Failure("TENANT_PLAN_NOT_FOUND", "Nenhum plano ativo encontrado para o tenant.");

        // 2. Valida plano do trial existe e está ativo
        var trialPlan = await _planRepository.GetByIdAsync(request.TrialPlanId, ct);
        if (trialPlan is null || !trialPlan.IsActive)
            return Result.Failure("PLAN_NOT_FOUND", "Plano de trial não encontrado ou inativo.");

        // 3. Valida que trial não é do plano Free — Free já é gratuito permanente (RN-029.3)
        if (trialPlan.Price == 0)
            return Result.Failure("TRIAL_OF_FREE_NOT_ALLOWED",
                "Não é possível iniciar um período de avaliação do plano gratuito.");

        // 4. Verifica se já usou trial deste plano — rejeita se sim
        var alreadyUsedTrial = await _tenantPlanRepository.HasUsedTrialForPlanAsync(
            request.TenantId, request.TrialPlanId, ct);
        if (alreadyUsedTrial)
            return Result.Failure("TRIAL_ALREADY_USED",
                "O tenant já utilizou o período de trial para este plano.");

        // 5. Valida Status != PendingCancellation
        if (activePlan.Status == TenantPlanStatus.PendingCancellation)
            return Result.Failure("PLAN_PENDING_CANCELLATION",
                "Não é possível iniciar um trial com cancelamento pendente. Reverta o cancelamento primeiro.");

        var today       = DateOnly.FromDateTime(DateTime.UtcNow);
        Guid? fallbackPlanId;

        // 6. Se TenantPlan atual é plano pago (IsTrial=false, Price>0): pausa o plano atual
        if (!activePlan.IsTrial && activePlan.Plan.Price > 0)
        {
            activePlan.Pause(today);
            _tenantPlanRepository.Update(activePlan);
            // FallbackPlanId do trial = PlanId do plano pausado (para retomada após o trial)
            fallbackPlanId = activePlan.PlanId;
        }
        else
        {
            // 7. TenantPlan atual é Free (Price=0) ou trial: expira o plano atual
            activePlan.Expire(today);
            _tenantPlanRepository.Update(activePlan);

            var freePlan = await _planRepository.GetFreePlanAsync(ct);
            fallbackPlanId = freePlan?.Id;
        }

        // 8. Cria novo TenantPlan de trial
        var newTrialPlan = TenantPlan.Create(
            tenantId:       request.TenantId,
            planId:         request.TrialPlanId,
            startDate:      today,
            endDate:        today.AddDays(30),
            isTrial:        true,
            fallbackPlanId: fallbackPlanId,
            status:         TenantPlanStatus.Active);

        await _tenantPlanRepository.AddAsync(newTrialPlan, ct);
        await _tenantPlanRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
