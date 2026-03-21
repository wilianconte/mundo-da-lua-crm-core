using MyCRM.Application.DTOs;
using MyCRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Queries.GetCustomerById;

public sealed class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public GetCustomerByIdHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(request.Id, ct);
        if (customer is null || customer.TenantId != _tenant.TenantId)
            return Result<CustomerDto>.Failure("CUSTOMER_NOT_FOUND", "Customer not found.");

        return Result<CustomerDto>.Success(customer.Adapt<CustomerDto>());
    }
}
