using MyCRM.Auth.Application.Commands.Plans.TerminateTrial;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="TerminateTrialHandler"/>.
///
/// Regra de negócio coberta (RN-029.9): encerramento de um período de trial.
/// Se havia um plano pago pausado, ele é retomado com os dias restantes preservados.
/// Se não havia plano pausado, o tenant faz downgrade: para Free (sem cobrança)
/// ou para um plano pago (com cobrança pelo valor cheio do primeiro mês).
/// </summary>
public sealed class TerminateTrialHandlerTests
{
    private readonly ITenantPlanRepository _tenantPlanRepo = Substitute.For<ITenantPlanRepository>();
    private readonly IBillingRepository    _billingRepo    = Substitute.For<IBillingRepository>();
    private readonly IPlanRepository       _planRepo       = Substitute.For<IPlanRepository>();
    private readonly TerminateTrialHandler _handler;

    public TerminateTrialHandlerTests()
    {
        _handler = new TerminateTrialHandler(_tenantPlanRepo, _billingRepo, _planRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Plan CreatePlan(string name, decimal price, bool isActive = true)
        => Plan.Create(name, name, price, isActive, 1);

    private static TenantPlan CreateTrialPlan(Guid tenantId, Plan plan)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tp = TenantPlan.Create(
            tenantId:       tenantId,
            planId:         plan.Id,
            startDate:      today.AddDays(-10),
            endDate:        today.AddDays(20),
            isTrial:        true,
            fallbackPlanId: null,
            status:         TenantPlanStatus.Active);

        typeof(TenantPlan).GetProperty(nameof(TenantPlan.Plan))!.SetValue(tp, plan);
        return tp;
    }

    private static TenantPlan CreatePausedPlan(Guid tenantId, Plan plan, int pausedDaysAgo = 10, int originalDaysLeft = 15)
    {
        var today     = DateOnly.FromDateTime(DateTime.UtcNow);
        var pausedAt  = today.AddDays(-pausedDaysAgo);
        // EndDate = a data em que o plano foi pausado + diasOriginaisRestantes
        // Para o handler: remainingDays = EndDate - PausedAt
        var endDate   = pausedAt.AddDays(originalDaysLeft);
        var tp = TenantPlan.Create(
            tenantId:       tenantId,
            planId:         plan.Id,
            startDate:      pausedAt.AddDays(-20),
            endDate:        endDate,
            isTrial:        false,
            fallbackPlanId: null,
            status:         TenantPlanStatus.Paused);

        typeof(TenantPlan).GetProperty(nameof(TenantPlan.Plan))!.SetValue(tp, plan);
        // Simula Pause() setando PausedAt via método de domínio
        tp.Pause(pausedAt);
        // Restaura o Status para Paused (Pause() já faz isso)
        return tp;
    }

    // ── Cenários de falha ────────────────────────────────────────────────────

    /// <summary>
    /// Problema: tenant sem plano ativo tenta encerrar trial.
    /// Sem plano ativo o handler não tem o que encerrar.
    /// </summary>
    [Fact]
    public async Task Handle_NoActivePlan_ReturnsTenantPlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta encerrar trial de um plano que não é trial.
    /// O handler rejeita para evitar encerramento acidental de planos pagos.
    /// </summary>
    [Fact]
    public async Task Handle_PlanNotTrial_ReturnsPlanNotTrial()
    {
        var tenantId = Guid.NewGuid();
        var plan     = CreatePlan("pro", 99m);
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var active   = TenantPlan.Create(
            tenantId, plan.Id, today.AddDays(-15), today.AddDays(15),
            false, null, TenantPlanStatus.Active);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_TRIAL", result.ErrorCode);
    }

    /// <summary>
    /// Problema: não há plano pausado para retomar e o tenant não informou
    /// um DowngradeToPlanId. O handler não sabe para qual plano migrar.
    /// </summary>
    [Fact]
    public async Task Handle_NoPausedPlanAndNoDowngradeId_ReturnsDowngradePlanRequired()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("DOWNGRADE_PLAN_REQUIRED", result.ErrorCode);
    }

    /// <summary>
    /// Problema: DowngradeToPlanId informado não existe ou está inativo.
    /// O tenant ficaria sem plano após o trial — não pode permitir.
    /// </summary>
    [Fact]
    public async Task Handle_DowngradePlanNotFound_ReturnsPlanNotFound()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("pro", 99m);
        var active    = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Plan?)null);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_FOUND", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: ao encerrar trial quando há plano pausado, os dias restantes
    /// do plano pausado devem ser calculados como (EndDate - PausedAt) e o novo
    /// EndDate deve ser hoje + esses dias. O handler preserva o período contratado
    /// tal como estava no momento da pausa — não desconta o tempo que o trial durou.
    /// </summary>
    [Fact]
    public async Task Handle_WithPausedPlan_ResumesPausedPlanWithRemainingDays()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("enterprise", 199m);
        var paidPlan  = CreatePlan("pro", 99m);
        var active    = CreateTrialPlan(tenantId, trialPlan);
        // pausado há 5 dias com 15 dias originais restantes (contados a partir de PausedAt)
        var paused = CreatePausedPlan(tenantId, paidPlan, pausedDaysAgo: 5, originalDaysLeft: 15);

        // Captura os valores antes que o handler modifique a entidade
        var pausedAt     = paused.PausedAt!.Value;
        var endDateAtPause = paused.EndDate!.Value;
        var remainingDays  = endDateAtPause.DayNumber - pausedAt.DayNumber; // = 15

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns(paused);

        var today  = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Active, paused.Status);
        Assert.Null(paused.PausedAt);
        Assert.Equal(today.AddDays(Math.Max(remainingDays, 1)), paused.EndDate);
    }

    /// <summary>
    /// Problema: ao encerrar trial sem plano pausado com downgrade para Free,
    /// não deve ser criada cobrança — o plano Free não tem custo.
    /// </summary>
    [Fact]
    public async Task Handle_DowngradeToFree_CreatesFreeTenantPlanWithoutBilling()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("pro", 99m);
        var freePlan  = CreatePlan("free", 0m);
        var active    = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(freePlan.Id, default).Returns(freePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        TenantPlan? createdPlan = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdPlan = tp), default);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, freePlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdPlan);
        Assert.False(createdPlan!.IsTrial);
        Assert.Null(createdPlan.EndDate);         // Free não tem EndDate
        Assert.Null(createdPlan.FallbackPlanId);  // Free não tem fallback
        await _billingRepo.DidNotReceive().AddAsync(Arg.Any<Billing>(), default);
    }

    /// <summary>
    /// Problema: ao encerrar trial sem plano pausado com downgrade para plano pago,
    /// deve ser criada cobrança pelo valor cheio do primeiro mês.
    /// </summary>
    [Fact]
    public async Task Handle_DowngradeToPaid_CreatesNewPlanWithFullPriceBilling()
    {
        var tenantId    = Guid.NewGuid();
        var trialPlan   = CreatePlan("enterprise", 199m);
        var downgradePlan = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(downgradePlan.Id, default).Returns(downgradePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        TenantPlan? createdPlan = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdPlan = tp), default);

        Billing? capturedBilling = null;
        await _billingRepo.AddAsync(Arg.Do<Billing>(b => capturedBilling = b), default);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, downgradePlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdPlan);
        Assert.False(createdPlan!.IsTrial);
        Assert.Equal(TenantPlanStatus.Active, createdPlan.Status);

        Assert.NotNull(capturedBilling);
        Assert.Equal(99m, capturedBilling!.Amount);
        Assert.Equal(BillingStatus.Pending, capturedBilling.Status);
    }

    /// <summary>
    /// Problema: o TenantPlan de trial deve ser marcado como Expirado ao ser encerrado,
    /// independente do caminho de downgrade escolhido.
    /// </summary>
    [Fact]
    public async Task Handle_Success_ExpiresTrialPlan()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("pro", 99m);
        var freePlan  = CreatePlan("free", 0m);
        var active    = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(freePlan.Id, default).Returns(freePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, freePlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Expired, active.Status);
    }

    /// <summary>
    /// Problema: verificar que todas as alterações são persistidas ao final.
    /// </summary>
    [Fact]
    public async Task Handle_Success_SavesChanges()
    {
        var tenantId  = Guid.NewGuid();
        var trialPlan = CreatePlan("pro", 99m);
        var freePlan  = CreatePlan("free", 0m);
        var active    = CreateTrialPlan(tenantId, trialPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(freePlan.Id, default).Returns(freePlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);

        var result = await _handler.Handle(
            new TerminateTrialCommand(tenantId, freePlan.Id), default);

        Assert.True(result.IsSuccess);
        await _tenantPlanRepo.Received(1).SaveChangesAsync(default);
    }
}
