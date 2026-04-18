using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.ReconcileTransaction;

public sealed class ReconcileTransactionHandler : IRequestHandler<ReconcileTransactionCommand, Result<ReconciliationDto>>
{
    private readonly ITransactionRepository    _transactions;
    private readonly IReconciliationRepository _reconciliations;
    private readonly ITenantService            _tenant;

    public ReconcileTransactionHandler(
        ITransactionRepository    transactions,
        IReconciliationRepository reconciliations,
        ITenantService            tenant)
    {
        _transactions    = transactions;
        _reconciliations = reconciliations;
        _tenant          = tenant;
    }

    public async Task<Result<ReconciliationDto>> Handle(ReconcileTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _transactions.GetByIdAsync(request.TransactionId, ct);
        if (transaction is null)
            return Result<ReconciliationDto>.Failure("TRANSACTION_NOT_FOUND", "Transaction not found.");

        if (transaction.IsReconciled)
            return Result<ReconciliationDto>.Failure("TRANSACTION_ALREADY_RECONCILED", "Transaction is already reconciled.");

        var reconciliation = Reconciliation.Create(
            _tenant.TenantId,
            transaction.WalletId,
            transaction.Id,
            request.ExternalId,
            request.ExternalAmount,
            request.ExternalDate);

        transaction.MarkAsReconciled();
        _transactions.Update(transaction);
        await _reconciliations.AddAsync(reconciliation, ct);
        await _reconciliations.SaveChangesAsync(ct);

        return Result<ReconciliationDto>.Success(reconciliation.Adapt<ReconciliationDto>());
    }
}
