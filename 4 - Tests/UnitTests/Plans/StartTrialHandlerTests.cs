using MyCRM.Auth.Application.Commands.Plans.StartTrial;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="StartTrialHandler"/>.
///
/// Regras de negócio cobertas (RN-029.3 e RN-029.14):
/// - Um tenant pode iniciar trial de um plano superior exatamente uma vez.
/// - Se o tenant está em plano pago, o plano atual é pausado (para ser retomado
///   após o trial); se está no Free, o plano é expirado.
/// - Não é permitido iniciar trial com cancelamento pendente.
/// </summary>
public sealed class StartTrialHandlerTests
{
    private readonly ITenantPlanRepository _tenantPlanRepo = Substitute.For<ITenantPlanRepository>();
    private readonly IPlanRepository       _planRepo       = Substitute.For<IPlanRepository>();
    private readonly StartTrialHandler     _handler;

    public StartTrialHandlerTests()
    {
        _handler = new StartTrialHandler(_tenantPlanRepo, _planRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Plan CreatePlan(string name, decimal price, bool isActive = true)
        => Plan.Create(name, name, price, isActive, 1);

    private static TenantPlan CreateTenantPlan(
        Guid tenantId,
        Plan plan,
        bool isTrial = false,
        TenantPlanStatus status = TenantPlanStatus.Active)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tp = TenantPlan.Create(
            tenantId:       tenantId,
            planId:         plan.Id,
            startDate:      today.AddDays(-15),
            endDate:        today.AddDays(15),
            isTrial:        isTrial,
            fallbackPlanId: null,
            status:         status);

        typeof(TenantPlan).GetProperty(nameof(TenantPlan.Plan))!.SetValue(tp, plan);
        return tp;
    }

    // ── Cenários de falha ────────────────────────────────────────────────────

    /// <summary>
    /// Problema: tenant sem plano ativo tenta iniciar trial.
    /// Sem plano ativo o estado do tenant é inconsistente — não permitir.
    /// </summary>
    [Fact]
    public async Task Handle_NoActivePlan_ReturnsTenantPlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta iniciar trial do plano Free (Price = 0).
    /// Trial é um benefício para avaliar planos pagos. O Free já é permanentemente
    /// gratuito — criar um TenantPlan Free com IsTrial=true e EndDate=+30d geraria
    /// um estado inconsistente (Free com prazo de vencimento).
    /// </summary>
    [Fact]
    public async Task Handle_TrialOfFreePlan_ReturnsTrialOfFreeNotAllowed()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var active    = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(freePlan.Id, default).Returns(freePlan);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, freePlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRIAL_OF_FREE_NOT_ALLOWED", result.ErrorCode);
    }

    /// <summary>
    /// Problema: o plano de trial informado não existe ou está inativo.
    /// Não deve ser possível iniciar trial de um plano inexistente ou descontinuado.
    /// </summary>
    [Fact]
    public async Task Handle_TrialPlanNotFound_ReturnsPlanNotFound()
    {
        var tenantId   = Guid.NewGuid();
        var freePlan   = CreatePlan("free", 0m);
        var active     = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Plan?)null);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: plano de trial existe mas está inativo.
    /// Mesmo cenário: não iniciar trial em plano fora de operação.
    /// </summary>
    [Fact]
    public async Task Handle_TrialPlanInactive_ReturnsPlanNotFound()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var trialPlan = CreatePlan("pro", 99m, isActive: false);
        var active    = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta iniciar trial de um plano que já utilizou anteriormente.
    /// O benefício do trial é concedido uma única vez por plano para evitar uso
    /// repetido de períodos gratuitos.
    /// </summary>
    [Fact]
    public async Task Handle_TrialAlreadyUsed_ReturnsTrialAlreadyUsed()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(true);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRIAL_ALREADY_USED", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant com cancelamento pendente tenta iniciar trial.
    /// Iniciar trial com cancelamento pendente criaria ambiguidade no estado do tenant:
    /// o que acontece com o plano ao fim do trial? O cancelamento deve ser revertido antes.
    /// </summary>
    [Fact]
    public async Task Handle_PendingCancellation_ReturnsPlanPendingCancellation()
    {
        var tenantId  = Guid.NewGuid();
        var plan      = CreatePlan("pro", 99m);
        var trialPlan = CreatePlan("enterprise", 199m);
        var active    = CreateTenantPlan(tenantId, plan, status: TenantPlanStatus.PendingCancellation);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(false);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_PENDING_CANCELLATION", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: ao iniciar trial a partir de um plano pago, o plano atual deve
    /// ser pausado (não expirado) para que possa ser retomado após o trial.
    /// O FallbackPlanId do trial deve apontar para o plano pausado.
    /// </summary>
    [Fact]
    public async Task Handle_FromPaidPlan_PausesCurrentPlanAndSetsFallbackToPaidPlan()
    {
        var tenantId  = Guid.NewGuid();
        var paidPlan  = CreatePlan("pro", 99m);
        var trialPlan = CreatePlan("enterprise", 199m);
        var active    = CreateTenantPlan(tenantId, paidPlan, isTrial: false);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(false);

        TenantPlan? createdTrial = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdTrial = tp), default);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Paused, active.Status);
        Assert.NotNull(active.PausedAt);

        Assert.NotNull(createdTrial);
        Assert.True(createdTrial!.IsTrial);
        Assert.Equal(TenantPlanStatus.Active, createdTrial.Status);
        // FallbackPlanId do trial = plano pago pausado
        Assert.Equal(active.PlanId, createdTrial.FallbackPlanId);
    }

    /// <summary>
    /// Problema: ao iniciar trial a partir do plano Free (preço = 0), o plano Free
    /// atual deve ser expirado (não pausado) e o FallbackPlanId do trial deve
    /// apontar para o Free — pois não há plano pago a retomar.
    /// </summary>
    [Fact]
    public async Task Handle_FromFreePlan_ExpiresCurrentPlanAndSetsFallbackToFree()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTenantPlan(tenantId, freePlan, isTrial: false);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(false);

        TenantPlan? createdTrial = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdTrial = tp), default);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Expired, active.Status);

        Assert.NotNull(createdTrial);
        Assert.True(createdTrial!.IsTrial);
        Assert.Equal(freePlan.Id, createdTrial.FallbackPlanId);
    }

    /// <summary>
    /// Problema: o novo TenantPlan de trial deve ter duração de 30 dias.
    /// Um trial com prazo incorreto geraria cobrança fora do período esperado.
    /// </summary>
    [Fact]
    public async Task Handle_Success_CreatesTenantPlanWithThirtyDayEndDate()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(false);

        TenantPlan? createdTrial = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdTrial = tp), default);

        await _handler.Handle(new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.NotNull(createdTrial);
        var expectedEnd = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        Assert.Equal(expectedEnd, createdTrial!.EndDate);
    }

    /// <summary>
    /// Problema: verificar que o novo plano de trial é persistido no banco.
    /// </summary>
    [Fact]
    public async Task Handle_Success_AddsNewTrialAndSaves()
    {
        var tenantId  = Guid.NewGuid();
        var freePlan  = CreatePlan("free", 0m);
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTenantPlan(tenantId, freePlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(trialPlan.Id, default).Returns(trialPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _tenantPlanRepo.HasUsedTrialForPlanAsync(tenantId, trialPlan.Id, default).Returns(false);

        var result = await _handler.Handle(
            new StartTrialCommand(tenantId, trialPlan.Id), default);

        Assert.True(result.IsSuccess);
        await _tenantPlanRepo.Received(1).AddAsync(Arg.Any<TenantPlan>(), default);
        await _tenantPlanRepo.Received(1).SaveChangesAsync(default);
    }
}
