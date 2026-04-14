using MediatR;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.RevertCancellation;

public sealed class RevertCancellationHandler : IRequestHandler<RevertCancellationCommand, Result>
{
    private readonly ITenantPlanRepository _tenantPlanRepository;
    private readonly IPlanRepository       _planRepository;

    public RevertCancellationHandler(
        ITenantPlanRepository tenantPlanRepository,
        IPlanRepository planRepository)
    {
        _tenantPlanRepository = tenantPlanRepository;
        _planRepository       = planRepository;
    }

    public async Task<Result> Handle(RevertCancellationCommand request, CancellationToken ct)
    {
        // 1. Busca TenantPlan Active
        var activePlan = await _tenantPlanRepository.GetActiveByTenantIdAsync(request.TenantId, ct);
        if (activePlan is null)
            return Result.Failure("TENANT_PLAN_NOT_FOUND", "Nenhum plano ativo encontrado para o tenant.");

        // 2. Valida Status = PendingCancellation
        if (activePlan.Status != TenantPlanStatus.PendingCancellation)
            return Result.Failure("PLAN_NOT_PENDING_CANCELLATION",
                "O plano não está com cancelamento pendente.");

        // 3. Valida EndDate > hoje
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (activePlan.EndDate.HasValue && activePlan.EndDate.Value <= today)
            return Result.Failure("PLAN_ALREADY_EXPIRED",
                "O plano já expirou e não pode ter o cancelamento revertido.");

        // 4. Busca o Free Plan para restaurar FallbackPlanId
        var freePlan = await _planRepository.GetFreePlanAsync(ct);

        activePlan.RevertCancellation(freePlan?.Id ?? Guid.Empty);
        _tenantPlanRepository.Update(activePlan);

        await _tenantPlanRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
