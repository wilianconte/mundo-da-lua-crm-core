using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Exceptions;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.Auth.Application.Services;

public sealed class PlanLimitService : IPlanLimitService
{
    private static readonly Dictionary<string, string> NumericMessages = new()
    {
        ["max_students"]  = "Limite de alunos do plano atingido. Faça upgrade para continuar.",
        ["max_employees"] = "Limite de funcionários do plano atingido. Faça upgrade para continuar.",
        ["max_courses"]   = "Limite de cursos do plano atingido. Faça upgrade para continuar.",
    };

    private const string BooleanDisabledMessage = "Esta funcionalidade não está disponível no seu plano atual.";

    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IPlanRepository _planRepository;

    public PlanLimitService(ITenantPlanRepository tenantPlanRepository, IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _planRepository       = planRepository;
    }

    public async Task CheckNumericLimitAsync(
        Guid tenantId,
        string featureKey,
        Func<Task<int>> getCurrentCountAsync,
        CancellationToken ct = default)
    {
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(tenantId, ct);
        if (activePlan is null)
            return; // sem plano ativo → ilimitado

        var planFeature = await _planRepository.GetPlanFeatureAsync(activePlan.PlanId, featureKey, ct);
        if (planFeature is null)
            return; // feature não configurada no plano → ilimitado (RN-028.6)

        if (planFeature.Value is null)
            return; // null = ilimitado

        var currentCount = await getCurrentCountAsync();
        if (currentCount >= planFeature.Value.Value)
        {
            var message = NumericMessages.TryGetValue(featureKey.ToLowerInvariant(), out var msg)
                ? msg
                : $"Limite de '{featureKey}' do plano atingido. Faça upgrade para continuar.";

            throw new PlanLimitException("PLAN_LIMIT_EXCEEDED", message);
        }
    }

    public async Task CheckBooleanFeatureAsync(
        Guid tenantId,
        string featureKey,
        CancellationToken ct = default)
    {
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(tenantId, ct);
        if (activePlan is null)
            return; // sem plano ativo → habilitado

        var planFeature = await _planRepository.GetPlanFeatureAsync(activePlan.PlanId, featureKey, ct);
        if (planFeature is null)
            return; // feature não configurada → habilitada (RN-028.6)

        if (planFeature.Value == 0)
            throw new PlanLimitException("PLAN_FEATURE_DISABLED", BooleanDisabledMessage);
    }
}
