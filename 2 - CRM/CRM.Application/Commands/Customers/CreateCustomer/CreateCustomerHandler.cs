using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Customers.CreateCustomer;

public sealed class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public CreateCustomerHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, ct: ct);
        if (emailExists)
            return Result<CustomerDto>.Failure("CUSTOMER_EMAIL_DUPLICATE", "A customer with this email already exists.");

        var customer = Customer.Create(
            tenantId: _tenant.TenantId,
            name: request.Name,
            email: request.Email,
            type: request.Type,
            phone: request.Phone,
            document: request.Document);

        await _repository.AddAsync(customer, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(customer.Adapt<CustomerDto>());
    }
}
