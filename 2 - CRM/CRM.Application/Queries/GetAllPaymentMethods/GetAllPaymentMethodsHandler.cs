using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllPaymentMethods;

public sealed class GetAllPaymentMethodsHandler : IRequestHandler<GetAllPaymentMethodsQuery, Result<IReadOnlyList<PaymentMethodDto>>>
{
    private readonly IPaymentMethodRepository _repository;

    public GetAllPaymentMethodsHandler(IPaymentMethodRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<PaymentMethodDto>>> Handle(GetAllPaymentMethodsQuery request, CancellationToken ct)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<PaymentMethodDto>>.Success(items.Adapt<IReadOnlyList<PaymentMethodDto>>());
    }
}
