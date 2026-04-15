using MyCRM.Auth.Application.Commands.Plans.MarkBillingAsPaid;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.Plans;

/// <summary>
/// Testes unitários para <see cref="MarkBillingAsPaidHandler"/>.
///
/// Regra de negócio coberta (RN-030.9): confirmação de pagamento de uma cobrança.
/// O handler valida a posse da cobrança (TenantId), o status atual e,
/// se o tenant estiver Suspenso, o reactiva automaticamente após o pagamento.
/// </summary>
public sealed class MarkBillingAsPaidHandlerTests
{
    private readonly IBillingRepository  _billingRepo = Substitute.For<IBillingRepository>();
    private readonly ITenantRepository   _tenantRepo  = Substitute.For<ITenantRepository>();
    private readonly MarkBillingAsPaidHandler _handler;

    public MarkBillingAsPaidHandlerTests()
    {
        _handler = new MarkBillingAsPaidHandler(_billingRepo, _tenantRepo);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Billing CreateBilling(
        Guid tenantId,
        BillingStatus status = BillingStatus.Pending)
    {
        var tenantPlanId = Guid.NewGuid();
        var billing = Billing.Create(
            tenantId:       tenantId,
            tenantPlanId:   tenantPlanId,
            amount:         99m,
            dueDate:        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10),
            referenceMonth: DateTime.UtcNow.ToString("yyyy-MM"));

        if (status == BillingStatus.Overdue)
            billing.MarkAsOverdue();
        else if (status == BillingStatus.Cancelled)
            billing.Cancel();
        else if (status == BillingStatus.Paid)
            billing.MarkAsPaid(DateTime.UtcNow.AddDays(-1));

        return billing;
    }

    // ── Cenários de falha ────────────────────────────────────────────────────

    /// <summary>
    /// Problema: cobrança com o BillingId informado não existe no banco.
    /// O handler deve retornar erro genérico sem vazar informação sobre a existência
    /// da cobrança (evitar enumeração de recursos).
    /// </summary>
    [Fact]
    public async Task Handle_BillingNotFound_ReturnsBillingNotFound()
    {
        var tenantId  = Guid.NewGuid();
        _billingRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Billing?)null);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(Guid.NewGuid(), tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BILLING_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: cobrança existe mas pertence a outro tenant.
    /// O handler deve retornar o mesmo erro de "não encontrado" para evitar
    /// que um tenant descubra cobranças de outro tenant (IDOR).
    /// </summary>
    [Fact]
    public async Task Handle_BillingBelongsToOtherTenant_ReturnsBillingNotFound()
    {
        var ownerTenantId    = Guid.NewGuid();
        var requestTenantId  = Guid.NewGuid(); // tenant diferente
        var billing = CreateBilling(ownerTenantId);

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, requestTenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BILLING_NOT_FOUND", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tentativa de marcar como paga uma cobrança que já foi paga.
    /// Pagamento duplicado deve ser rejeitado para evitar inconsistências financeiras.
    /// </summary>
    [Fact]
    public async Task Handle_BillingAlreadyPaid_ReturnsBillingCannotBePaid()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Paid);

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BILLING_CANNOT_BE_PAID", result.ErrorCode);
    }

    /// <summary>
    /// Problema: tentativa de marcar como paga uma cobrança cancelada.
    /// Uma cobrança cancelada não pode ser reaberta como paga.
    /// </summary>
    [Fact]
    public async Task Handle_BillingCancelled_ReturnsBillingCannotBePaid()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Cancelled);

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("BILLING_CANNOT_BE_PAID", result.ErrorCode);
    }

    // ── Cenários de sucesso ──────────────────────────────────────────────────

    /// <summary>
    /// Problema: cobrança Pending deve ser marcada como Paid com o timestamp correto.
    /// O registro de PaidAt é essencial para auditoria financeira.
    /// </summary>
    [Fact]
    public async Task Handle_PendingBilling_MarksAsPaidWithTimestamp()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Pending);
        var tenant   = Tenant.Create("Acme", Guid.NewGuid());

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);
        _tenantRepo.GetByIdAsync(tenantId, default).Returns(tenant);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(BillingStatus.Paid, billing.Status);
        Assert.NotNull(billing.PaidAt);
    }

    /// <summary>
    /// Problema: cobrança Overdue (em atraso) também deve poder ser marcada como paga,
    /// pois o tenant pode pagar após a data de vencimento.
    /// </summary>
    [Fact]
    public async Task Handle_OverdueBilling_CanBeMarkedAsPaid()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Overdue);
        var tenant   = Tenant.Create("Acme", Guid.NewGuid());

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);
        _tenantRepo.GetByIdAsync(tenantId, default).Returns(tenant);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(BillingStatus.Paid, billing.Status);
    }

    /// <summary>
    /// Problema: tenant com Status = Suspended deve ter sua conta reativada
    /// automaticamente ao confirmar pagamento. A suspensão geralmente ocorre por
    /// inadimplência, e o pagamento deve restaurar o acesso imediatamente.
    /// </summary>
    [Fact]
    public async Task Handle_SuspendedTenant_ActivatesTenantAfterPayment()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Pending);
        var tenant   = Tenant.Create("Acme", Guid.NewGuid());
        tenant.Suspend(); // coloca em Suspended

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);
        _tenantRepo.GetByIdAsync(tenantId, default).Returns(tenant);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Active, tenant.Status);
        _tenantRepo.Received(1).Update(tenant);
    }

    /// <summary>
    /// Problema: tenant que já está Active não deve ter seu status alterado ao pagar.
    /// O Update no tenant só deve ocorrer quando o tenant estava Suspended.
    /// </summary>
    [Fact]
    public async Task Handle_ActiveTenant_DoesNotCallTenantUpdate()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Pending);
        var tenant   = Tenant.Create("Acme", Guid.NewGuid()); // já Active por padrão

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);
        _tenantRepo.GetByIdAsync(tenantId, default).Returns(tenant);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.True(result.IsSuccess);
        _tenantRepo.DidNotReceive().Update(Arg.Any<Tenant>());
    }

    /// <summary>
    /// Problema: verificar que o estado da cobrança é persistido após o pagamento.
    /// Sem Update + SaveChangesAsync o status Paid não seria salvo no banco.
    /// </summary>
    [Fact]
    public async Task Handle_ValidPayment_CallsUpdateAndSave()
    {
        var tenantId = Guid.NewGuid();
        var billing  = CreateBilling(tenantId, BillingStatus.Pending);
        var tenant   = Tenant.Create("Acme", Guid.NewGuid());

        _billingRepo.GetByIdAsync(billing.Id, default).Returns(billing);
        _tenantRepo.GetByIdAsync(tenantId, default).Returns(tenant);

        var result = await _handler.Handle(
            new MarkBillingAsPaidCommand(billing.Id, tenantId), default);

        Assert.True(result.IsSuccess);
        _billingRepo.Received(1).Update(billing);
        await _billingRepo.Received(1).SaveChangesAsync(default);
    }
}
