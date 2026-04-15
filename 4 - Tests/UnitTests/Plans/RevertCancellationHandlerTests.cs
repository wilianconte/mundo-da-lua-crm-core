using MyCRM.Auth.Application.Commands.Plans.RevertCancellation;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="RevertCancellationHandler"/>.
///
/// Regra de negócio coberta (RN-029.11): o tenant pode desfazer um pedido de
/// cancelamento enquanto o plano ainda está vigente (EndDate > hoje). O plano
/// retorna para Status = Active e o FallbackPlanId é redefinido para o plano Free.
/// </summary>
public sealed class RevertCancellationHandlerTests
{
    private readonly ITenantPlanRepository _tenantPlanRepo = Substitute.For<ITenantPlanRepository>();
    private readonly IPlanRepository       _planRepo       = Substitute.For<IPlanRepository>();
    private readonly RevertCancellationHandler _handler;

    public RevertCancellationHandlerTests()
    {
        _handler = new RevertCancellationHandler(_tenantPlanRepo, _planRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Plan CreateFreePlan()
        => Plan.Create("free", "Gratuito", 0m, true, 1);

    private static TenantPlan CreatePendingCancellationPlan(Guid tenantId, int daysUntilEnd = 10)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan  = Plan.Create("pro", "Pro", 99m);
        var tp    = TenantPlan.Create(
            tenantId:       tenantId,
            planId:         plan.Id,
            startDate:      today.AddDays(-20),
            endDate:        today.AddDays(daysUntilEnd),
            isTrial:        false,
            fallbackPlanId: Guid.NewGuid(),
            status:         TenantPlanStatus.PendingCancellation);

        typeof(TenantPlan).GetProperty(nameof(TenantPlan.Plan))!.SetValue(tp, plan);
        return tp;
    }

    // ── Cenários de falha ────────────────────────────────────────────────────

    /// <summary>
    /// Problema: tenant sem plano ativo tenta reverter cancelamento.
    /// Sem plano ativo não há cancelamento pendente a reverter.
    /// </summary>
    [Fact]
    public async Task Handle_NoActivePlan_ReturnsTenantPlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta reverter cancelamento de um plano que está com
    /// Status = Active (não em PendingCancellation). Só faz sentido reverter
    /// quando há um cancelamento pendente.
    /// </summary>
    [Fact]
    public async Task Handle_PlanNotPendingCancellation_ReturnsPlanNotPendingCancellation()
    {
        var tenantId = Guid.NewGuid();
        var plan     = Plan.Create("pro", "Pro", 99m);
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var active   = TenantPlan.Create(
            tenantId, plan.Id, today.AddDays(-15), today.AddDays(15),
            false, null, TenantPlanStatus.Active);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);

        var result = await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_PENDING_CANCELLATION", result.ErrorCode);
    }

    /// <summary>
    /// Problema: plano em PendingCancellation já atingiu ou passou do EndDate.
    /// Após o vencimento não há mais nada a reverter — o período de uso acabou.
    /// </summary>
    [Fact]
    public async Task Handle_PlanAlreadyExpired_ReturnsPlanAlreadyExpired()
    {
        var tenantId = Guid.NewGuid();
        // EndDate = hoje (já vencido — today <= today)
        var expiredPlan = CreatePendingCancellationPlan(tenantId, daysUntilEnd: 0);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(expiredPlan);

        var result = await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_ALREADY_EXPIRED", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: ao reverter, o TenantPlan deve voltar ao Status = Active e
    /// o CancelledAt deve ser limpo, como se o cancelamento nunca tivesse ocorrido.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRevert_ReactivatesPlanAndClearsCancelledAt()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreateFreePlan();
        var activePlan = CreatePendingCancellationPlan(tenantId, daysUntilEnd: 10);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(activePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        var result = await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Active, activePlan.Status);
        Assert.Null(activePlan.CancelledAt);
    }

    /// <summary>
    /// Problema: ao reverter, o FallbackPlanId deve ser redefinido para o plano Free.
    /// O FallbackPlanId anterior era o plano de downgrade escolhido no momento do
    /// cancelamento — após a reversão, esse plano de downgrade não é mais aplicável.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRevert_SetsFallbackToFreePlan()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreateFreePlan();
        var activePlan = CreatePendingCancellationPlan(tenantId, daysUntilEnd: 10);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(activePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.Equal(freePlan.Id, activePlan.FallbackPlanId);
    }

    /// <summary>
    /// Problema: verificar que o estado revertido é persistido.
    /// Sem Update + SaveChangesAsync a reversão não seria salva no banco.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRevert_CallsUpdateAndSave()
    {
        var tenantId   = Guid.NewGuid();
        var freePlan   = CreateFreePlan();
        var activePlan = CreatePendingCancellationPlan(tenantId, daysUntilEnd: 10);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(activePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        var result = await _handler.Handle(new RevertCancellationCommand(tenantId), default);

        Assert.True(result.IsSuccess);
        _tenantPlanRepo.Received(1).Update(activePlan);
        await _tenantPlanRepo.Received(1).SaveChangesAsync(default);
    }
}
