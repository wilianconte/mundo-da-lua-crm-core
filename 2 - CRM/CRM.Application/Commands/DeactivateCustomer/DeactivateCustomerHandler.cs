using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerHandler : IRequestHandler<DeactivateCustomerCommand, Result>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public DeactivateCustomerHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result> Handle(DeactivateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(request.Id, ct);
        if (customer is null || customer.TenantId != _tenant.TenantId)
            return Result.Failure("CUSTOMER_NOT_FOUND", "Customer not found.");

        customer.Deactivate();
        _repository.Update(customer);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
