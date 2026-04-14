using MediatR;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.MarkBillingAsPaid;

public sealed class MarkBillingAsPaidHandler : IRequestHandler<MarkBillingAsPaidCommand, Result>
{
    private readonly IBillingRepository  _billingRepository;
    private readonly ITenantRepository   _tenantRepository;

    public MarkBillingAsPaidHandler(
        IBillingRepository billingRepository,
        ITenantRepository tenantRepository)
    {
        _billingRepository = billingRepository;
        _tenantRepository  = tenantRepository;
    }

    public async Task<Result> Handle(MarkBillingAsPaidCommand request, CancellationToken ct)
    {
        // 1. Busca Billing por Id — erro se não encontrar ou TenantId não bater
        var billing = await _billingRepository.GetByIdAsync(request.BillingId, ct);
        if (billing is null)
            return Result.Failure("BILLING_NOT_FOUND", "Cobrança não encontrada.");

        if (billing.TenantId != request.TenantId)
            return Result.Failure("BILLING_NOT_FOUND", "Cobrança não encontrada.");

        // 2. Valida Status = Pending ou Overdue
        if (billing.Status is not (BillingStatus.Pending or BillingStatus.Overdue))
            return Result.Failure("BILLING_CANNOT_BE_PAID",
                "A cobrança não pode ser marcada como paga no status atual.");

        // 3. Billing: Status = Paid, PaidAt = DateTime.UtcNow
        billing.MarkAsPaid(DateTime.UtcNow);
        _billingRepository.Update(billing);

        // 4. Se Tenant.Status = Suspended: Tenant.Status = Active
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is not null && tenant.Status == TenantStatus.Suspended)
        {
            tenant.Activate();
            _tenantRepository.Update(tenant);
        }

        await _billingRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
