using MyCRM.Auth.Application.Commands.Plans.UpgradeTenantPlan;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="UpgradeTenantPlanHandler"/>.
///
/// Regra de negócio coberta (RN-029.6): quando um tenant solicita upgrade de plano,
/// o plano ativo anterior deve ser marcado como Upgraded, qualquer plano Paused deve
/// ser cancelado, e um novo TenantPlan é criado com cobrança proporcional ao tempo
/// restante (se origem paga) ou pelo valor cheio (se origem trial).
/// </summary>
public sealed class UpgradeTenantPlanHandlerTests
{
    private readonly ITenantPlanRepository _tenantPlanRepo = Substitute.For<ITenantPlanRepository>();
    private readonly IBillingRepository    _billingRepo    = Substitute.For<IBillingRepository>();
    private readonly IPlanRepository       _planRepo       = Substitute.For<IPlanRepository>();
    private readonly UpgradeTenantPlanHandler _handler;

    public UpgradeTenantPlanHandlerTests()
    {
        _handler = new UpgradeTenantPlanHandler(_tenantPlanRepo, _billingRepo, _planRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Plan CreatePlan(string name = "pro", decimal price = 99m, bool isActive = true)
        => Plan.Create(name, name, price, isActive, 1);

    private static TenantPlan CreateTenantPlan(
        Guid tenantId,
        Plan plan,
        bool isTrial = false,
        TenantPlanStatus status = TenantPlanStatus.Active,
        int daysBack = 15,
        int daysForward = 15)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tp = TenantPlan.Create(
            tenantId:       tenantId,
            planId:         plan.Id,
            startDate:      today.AddDays(-daysBack),
            endDate:        today.AddDays(daysForward),
            isTrial:        isTrial,
            fallbackPlanId: null,
            status:         status);

        // Injeta a propriedade de navegação Plan via reflection (privada no domínio)
        typeof(TenantPlan).GetProperty(nameof(TenantPlan.Plan))!.SetValue(tp, plan);
        return tp;
    }

    // ── Cenários de falha ────────────────────────────────────────────────────

    /// <summary>
    /// Problema: tenant tenta fazer upgrade diretamente para o plano Free.
    /// O caminho correto de downgrade para Free é via CancelTenantPlan (respeita o
    /// período pago até EndDate). Upgrade direto pularia esse período e violaria RN-028.3,
    /// que define que TenantPlan Free deve ter EndDate = null.
    /// </summary>
    [Fact]
    public async Task Handle_UpgradeToFreePlan_ReturnsUpgradeToFreeNotAllowed()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(freePlan.Id, default).Returns(freePlan);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, freePlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("UPGRADE_TO_FREE_NOT_ALLOWED", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant sem plano ativo tenta fazer upgrade.
    /// O handler deve retornar falha imediata sem criar nenhum plano ou cobrança.
    /// </summary>
    [Fact]
    public async Task Handle_NoActivePlan_ReturnsTenantPlanNotFound()
    {
        var tenantId = Guid.NewGuid();
        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TENANT_PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: o plano de destino do upgrade não existe ou está inativo.
    /// O sistema não pode migrar para um plano inexistente/desabilitado.
    /// </summary>
    [Fact]
    public async Task Handle_NewPlanNotFound_ReturnsPlanNotFound()
    {
        var tenantId   = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var active      = CreateTenantPlan(tenantId, currentPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Plan?)null);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: plano de destino existe mas está inativo.
    /// O handler deve rejeitar o upgrade para evitar onboarding em planos descontinuados.
    /// </summary>
    [Fact]
    public async Task Handle_NewPlanInactive_ReturnsPlanNotFound()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 99m, isActive: false);
        var active      = CreateTenantPlan(tenantId, currentPlan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant com cancelamento pendente tenta fazer upgrade sem antes reverter o cancelamento.
    /// Pela RN-029.10, o status PendingCancellation bloqueia qualquer upgrade — o tenant deve
    /// chamar RevertCancellation primeiro para restaurar o plano ao estado Active.
    /// </summary>
    [Fact]
    public async Task Handle_PlanPendingCancellation_ReturnsUpgradeBlockedPendingCancellation()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("pro", 99m);
        var newPlan     = CreatePlan("enterprise", 199m);
        var active      = CreateTenantPlan(tenantId, currentPlan,
            status: TenantPlanStatus.PendingCancellation);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("UPGRADE_BLOCKED_PENDING_CANCELLATION", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tenant tenta fazer "upgrade" para o mesmo plano que já possui.
    /// Deve ser rejeitado para evitar duplicidade de cobranças e planos.
    /// </summary>
    [Fact]
    public async Task Handle_SamePlan_ReturnsPlanSameAsCurrent()
    {
        var tenantId    = Guid.NewGuid();
        var plan        = CreatePlan("pro", 99m);
        var active      = CreateTenantPlan(tenantId, plan);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _planRepo.GetByIdAsync(plan.Id, default).Returns(plan);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, plan.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PLAN_SAME_AS_CURRENT", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: upgrade a partir de trial deve gerar cobrança pelo valor cheio
    /// do novo plano, pois o tenant ainda não pagou nada pelo período anterior.
    /// </summary>
    [Fact]
    public async Task Handle_UpgradeFromTrial_GeneratesFullPriceBilling()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("free", 0m);
        var newPlan     = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: true);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);

        Billing? capturedBilling = null;
        await _billingRepo.AddAsync(Arg.Do<Billing>(b => capturedBilling = b), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedBilling);
        Assert.Equal(99m, capturedBilling!.Amount);
        Assert.Equal(BillingStatus.Pending, capturedBilling.Status);
    }

    /// <summary>
    /// Problema: upgrade a partir de plano pago deve gerar cobrança proporcional
    /// aos dias restantes do plano atual. Cobra-se apenas pelo novo plano no período
    /// não coberto pelo plano anterior, evitando cobrança dupla.
    /// </summary>
    [Fact]
    public async Task Handle_UpgradeFromPaidPlan_GeneratesProportionalBilling()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 100m);
        var freePlan    = CreatePlan("free", 0m);
        // Plano atual: 20 dias no total, 10 dias restantes → 50% de 100 = 50
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: false, daysBack: 10, daysForward: 10);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);

        Billing? capturedBilling = null;
        await _billingRepo.AddAsync(Arg.Do<Billing>(b => capturedBilling = b), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedBilling);
        // 10 dias restantes / 20 dias totais × 100 = 50,00
        Assert.Equal(50m, capturedBilling!.Amount);
    }

    /// <summary>
    /// Problema: ao fazer upgrade de um plano pago, cobranças pendentes do plano
    /// anterior devem ser canceladas para evitar cobrança dupla.
    /// </summary>
    [Fact]
    public async Task Handle_UpgradeFromPaidPlan_CancelsPendingBillings()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: false);
        var pendingBilling = Billing.Create(tenantId, active.Id, 49m,
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), "2026-04");

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing> { pendingBilling }.AsReadOnly() as IReadOnlyList<Billing>);
        await _billingRepo.AddAsync(Arg.Any<Billing>(), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(BillingStatus.Cancelled, pendingBilling.Status);
        _billingRepo.Received(1).Update(pendingBilling);
    }

    /// <summary>
    /// Problema: se o tenant possui um plano Paused (proveniente de um trial ativo),
    /// ao fazer upgrade esse plano pausado deve ser cancelado — ele não faz mais
    /// sentido quando um novo plano pago é contratado diretamente.
    /// </summary>
    [Fact]
    public async Task Handle_WithPausedPlan_CancelsPausedPlan()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: true);
        var paused      = CreateTenantPlan(tenantId, currentPlan, status: TenantPlanStatus.Paused);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns(paused);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);
        await _billingRepo.AddAsync(Arg.Any<Billing>(), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Cancelled, paused.Status);
        _tenantPlanRepo.Received(1).Update(paused);
    }

    /// <summary>
    /// Problema: ao finalizar o upgrade com sucesso, o plano ativo anterior deve
    /// ter seu status alterado para Upgraded e ser persistido no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_Success_MarksOldPlanAsUpgradedAndSaves()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: true);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);
        await _billingRepo.AddAsync(Arg.Any<Billing>(), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantPlanStatus.Upgraded, active.Status);
        _tenantPlanRepo.Received(1).Update(active);
        await _tenantPlanRepo.Received(1).SaveChangesAsync(default);
    }

    /// <summary>
    /// Problema: o novo TenantPlan criado após o upgrade deve ter IsTrial = false,
    /// Status = Active e EndDate = hoje + 1 mês, garantindo o ciclo de cobrança mensal.
    /// </summary>
    [Fact]
    public async Task Handle_Success_CreatesNewActivePaidTenantPlan()
    {
        var tenantId    = Guid.NewGuid();
        var currentPlan = CreatePlan("basic", 49m);
        var newPlan     = CreatePlan("pro", 99m);
        var freePlan    = CreatePlan("free", 0m);
        var active      = CreateTenantPlan(tenantId, currentPlan, isTrial: true);

        _tenantPlanRepo.GetActiveByTenantIdAsync(tenantId, default).Returns(active);
        _tenantPlanRepo.GetPausedByTenantIdAsync(tenantId, default).Returns((TenantPlan?)null);
        _planRepo.GetByIdAsync(newPlan.Id, default).Returns(newPlan);
        _planRepo.GetFreePlanAsync(default).Returns(freePlan);
        _billingRepo.GetAllPendingByTenantPlanIdAsync(active.Id, default)
            .Returns(new List<Billing>().AsReadOnly() as IReadOnlyList<Billing>);
        await _billingRepo.AddAsync(Arg.Any<Billing>(), default);

        TenantPlan? createdPlan = null;
        await _tenantPlanRepo.AddAsync(Arg.Do<TenantPlan>(tp => createdPlan = tp), default);

        var result = await _handler.Handle(
            new UpgradeTenantPlanCommand(tenantId, newPlan.Id), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(createdPlan);
        Assert.Equal(tenantId, createdPlan!.TenantId);
        Assert.Equal(newPlan.Id, createdPlan.PlanId);
        Assert.False(createdPlan.IsTrial);
        Assert.Equal(TenantPlanStatus.Active, createdPlan.Status);
    }
}
