using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllTransactions;

public sealed class GetAllTransactionsHandler : IRequestHandler<GetAllTransactionsQuery, Result<IReadOnlyList<TransactionDto>>>
{
    private readonly ITransactionRepository _repository;

    public GetAllTransactionsHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(GetAllTransactionsQuery request, CancellationToken ct)
    {
        var query = _repository.Query();

        if (request.WalletId.HasValue)
            query = query.Where(t => t.WalletId == request.WalletId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= request.EndDate.Value);

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == request.Type.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (request.PaymentMethodId.HasValue)
            query = query.Where(t => t.PaymentMethodId == request.PaymentMethodId.Value);

        var items = query.ToList();
        return Result<IReadOnlyList<TransactionDto>>.Success(items.Adapt<IReadOnlyList<TransactionDto>>());
    }
}
