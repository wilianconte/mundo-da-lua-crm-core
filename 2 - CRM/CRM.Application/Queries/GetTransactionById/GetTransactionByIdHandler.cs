using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetTransactionById;

public sealed class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto?>>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionByIdHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<Result<TransactionDto?>> Handle(GetTransactionByIdQuery request, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, ct);
        return Result<TransactionDto?>.Success(transaction?.Adapt<TransactionDto>());
    }
}
