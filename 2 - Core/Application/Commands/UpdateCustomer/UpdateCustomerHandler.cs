using MyCRM.Application.DTOs;
using MyCRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.UpdateCustomer;

public sealed class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public UpdateCustomerHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(request.Id, ct);
        if (customer is null || customer.TenantId != _tenant.TenantId)
            return Result<CustomerDto>.Failure("CUSTOMER_NOT_FOUND", "Customer not found.");

        var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, request.Id, ct);
        if (emailExists)
            return Result<CustomerDto>.Failure("CUSTOMER_EMAIL_DUPLICATE", "A customer with this email already exists.");

        customer.Update(request.Name, request.Email, request.Phone, request.Document, request.Notes);
        _repository.Update(customer);
        await _repository.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(customer.Adapt<CustomerDto>());
    }
}
