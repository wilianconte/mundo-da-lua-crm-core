using MediatR;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.DeleteTransaction;

public sealed class DeleteTransactionHandler : IRequestHandler<DeleteTransactionCommand, Result>
{
    private readonly ITransactionRepository _repository;

    public DeleteTransactionHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, ct);
        if (transaction is null)
            return Result.Failure("TRANSACTION_NOT_FOUND", "Transaction not found.");

        if (transaction.IsReconciled)
            return Result.Failure("TRANSACTION_RECONCILED", "Cannot delete a reconciled transaction.");

        transaction.SoftDelete();
        _repository.Update(transaction);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
