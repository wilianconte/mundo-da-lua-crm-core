using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.UpdateTransaction;

public sealed class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Result<TransactionDto>>
{
    private readonly ITransactionRepository _repository;

    public UpdateTransactionHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<Result<TransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, ct);
        if (transaction is null)
            return Result<TransactionDto>.Failure("TRANSACTION_NOT_FOUND", "Transaction not found.");

        if (transaction.IsReconciled)
            return Result<TransactionDto>.Failure("TRANSACTION_RECONCILED", "Cannot edit a reconciled transaction.");

        transaction.Update(
            request.Amount,
            request.Description,
            request.CategoryId,
            request.PaymentMethodId,
            request.TransactionDate);

        _repository.Update(transaction);
        await _repository.SaveChangesAsync(ct);

        return Result<TransactionDto>.Success(transaction.Adapt<TransactionDto>());
    }
}
