using MyCRM.Application.DTOs;
using MyCRM.Domain.Entities;
using MyCRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.SetCustomerAddress;

public sealed class SetCustomerAddressHandler : IRequestHandler<SetCustomerAddressCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public SetCustomerAddressHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CustomerDto>> Handle(SetCustomerAddressCommand request, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(request.Id, ct);
        if (customer is null || customer.TenantId != _tenant.TenantId)
            return Result<CustomerDto>.Failure("CUSTOMER_NOT_FOUND", "Customer not found.");

        var address = new Address
        {
            Street = request.Street,
            Number = request.Number,
            Complement = request.Complement,
            Neighborhood = request.Neighborhood,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country
        };

        customer.SetAddress(address);
        _repository.Update(customer);
        await _repository.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(customer.Adapt<CustomerDto>());
    }
}
