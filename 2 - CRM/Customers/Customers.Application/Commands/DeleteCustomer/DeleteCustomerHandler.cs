using MyCRM.Customers.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.DeleteCustomer;

public sealed class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantService _tenant;

    public DeleteCustomerHandler(ICustomerRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(request.Id, ct);
        if (customer is null || customer.TenantId != _tenant.TenantId)
            return Result.Failure("CUSTOMER_NOT_FOUND", "Customer not found.");

        _repository.Delete(customer);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
