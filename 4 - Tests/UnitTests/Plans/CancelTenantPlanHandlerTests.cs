using MyCRM.Auth.Application.Commands.Plans.CancelTenantPlan;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="CancelTenantPlanHandler"/>.
///
/// Regra de negócio coberta (RN-029.7): cancelamento de plano com downgrade
/// programado. O plano entra em PendingCancellation — permanece ativo até o
/// fim do período vigente — e todas as cobranças pendentes são canceladas para
/// evitar cobrança após a solicitação de cancelamento.
/// </summary>
public sealed class CancelTenantPlanHandlerTests
{
    private readonly ITenantPlanRepository _tenantPlanRepo = Substitute.For<ITenantPlanRepository>();
    private readonly IBillingRepository    _billingRepo    = Substitute.For<IBillingRepository>();
    private readonly IPlanRepository       _planRepo       = Substitute.For<IPlanRepository>();
    private readonly CancelTenantPlanHandler _handler;

    public CancelTenantPlanHandlerTests()
    {
        _handler = new CancelTenantPlanHandler(_tenantPlanRepo, _billingRepo, _planRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Plan CreatePlan(string name = "pro", decimal price = 99m, bool isActive = true)
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
    /// Problema: tenant sem plano ativo tenta cancelar.
    /// Sem plano ativo não há nada a cancelar — o handler deve falhar imediatamente.
    /// </summary>
    [Fact]
    public async Task Handle_NoActivePlan_ReturnsTenantPlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta cancelar um plano que ainda está em trial.
    /// Trials devem ser encerrados via TerminateTrial, não via CancelTenantPlan,
    /// para garantir o fluxo correto de retomada do plano pausado.
    /// </summary>
    [Fact]
    public async Task Handle_TrialPlan_ReturnsCancelTrialNotAllowed()
    {
        var tenantId = Guid.NewGuid();
        var plan     = CreatePlan("pro", 99m);
        var active   = CreateTenantPlan(tenantId, plan, isTrial: true);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_CANCEL_TRIAL_NOT_ALLOWED", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta cancelar um plano que já está em PendingCancellation.
    /// O plano não está com Status = Active, portanto a operação deve ser rejeitada
    /// para evitar modificações de estado inconsistentes.
    /// </summary>
    [Fact]
    public async Task Handle_PlanNotActive_ReturnsPlanNotActive()
    {
        var tenantId = Guid.NewGuid();
        var plan     = CreatePlan("pro", 99m);
        var active   = CreateTenantPlan(tenantId, plan, isTrial: false,
            status: TenantPlanStatus.PendingCancellation);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_ACTIVE", result.ErrorCode);
    }

    /// <summary>
    /// Problema: o plano de downgrade informado não existe ou está inativo.
    /// O sistema precisa saber para qual plano migrar ao fim do período para
    /// evitar que o tenant fique sem plano.
    /// </summary>
    [Fact]
    public async Task Handle_DowngradePlanNotFound_ReturnsDowngradePlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        var plan     = CreatePlan("pro", 99m);
        var active   = CreateTenantPlan(tenantId, plan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Plan?)null);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("DOWNGRADE_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: plano de downgrade existe mas está inativo.
    /// Não deve ser possível programar downgrade para planos fora de operação.
    /// </summary>
    [Fact]
    public async Task Handle_DowngradePlanInactive_ReturnsDowngradePlanNotFound()
    {
        var tenantId      = Guid.NewGuid();
        var plan          = CreatePlan("pro", 99m);
        var downgradePlan = CreatePlan("basic", 49m, isActive: false);
        var active        = CreateTenantPlan(tenantId, plan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(downgradePlan.Id, default).Returns(downgradePlan);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, downgradePlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("DOWNGRADE_PLAN_NOT_FOUND", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: ao solicitar cancelamento, o TenantPlan deve transitar para
    /// PendingCancellation e o FallbackPlanId deve ser definido para o plano
    /// de downgrade escolhido pelo tenant.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCancel_SetsPendingCancellationWithFallback()
    {
        var tenantId      = Guid.NewGuid();
        var plan          = CreatePlan("pro", 99m);
        var downgradePlan = CreatePlan("free", 0m);
        var active        = CreateTenantPlan(tenantId, plan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(downgradePlan.Id, default).Returns(downgradePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, downgradePlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.PendingCancellation, active.Status);
        Assert.Equal(downgradePlan.Id, active.FallbackPlanId);
        Assert.NotNull(active.CancelledAt);
    }

    /// <summary>
    /// Problema: ao cancelar, cobranças pendentes vinculadas ao TenantPlan devem
    /// ser canceladas. Manter cobranças pendentes de um plano em cancelamento
    /// geraria cobrança indevida ao cliente.
    /// </summary>
    [Fact]
    public async Task Handle_WithPendingBillings_CancelsAllPendingBillings()
    {
        var tenantId      = Guid.NewGuid();
        var plan          = CreatePlan("pro", 99m);
        var downgradePlan = CreatePlan("free", 0m);
        var active        = CreateTenantPlan(tenantId, plan);
        var today         = DateOnly.FromDateTime(DateTime.UtcNow);
        var bill1         = Billing.Create(tenantId, active.Id, 99m, today.AddDays(5), "2026-04");
        var bill2         = Billing.Create(tenantId, active.Id, 99m, today.AddDays(35), "2026-05");

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(downgradePlan.Id, default).Returns(downgradePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing> { bill1, bill2 }.AsReadOnly() as IReadOnlyList<Billing>);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, downgradePlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(BillingStatus.Cancelled, bill1.Status);
        Assert.Equal(BillingStatus.Cancelled, bill2.Status);
        _billingRepo.Received(1).Update(bill1);
        _billingRepo.Received(1).Update(bill2);
    }

    /// <summary>
    /// Problema: verificar que as alterações são persistidas ao fim do cancelamento.
    /// Sem o SaveChangesAsync, todas as mudanças seriam perdidas.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCancel_CallsUpdateAndSave()
    {
        var tenantId      = Guid.NewGuid();
        var plan          = CreatePlan("pro", 99m);
        var downgradePlan = CreatePlan("free", 0m);
        var active        = CreateTenantPlan(tenantId, plan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(downgradePlan.Id, default).Returns(downgradePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);

        var result = await _handler.Handle(
            new CancelTenantPlanCommand(tenantId, downgradePlan.Id), default);

        Assert.True(result.IsSuccess);
        _tenantPlanRepo.Received(1).Update(active);
        await _tenantPlanRepo.Received(1).SaveChangesAsync(default);
    }
}
