using MediatR;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Tenants.DeleteTenant;

public sealed class DeleteTenantHandler : IRequestHandler<DeleteTenantCommand, Result>
{
    private readonly ITenantRepository _repository;

    public DeleteTenantHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken ct)
    {
        var tenant = await _repository.GetByIdAsync(request.Id, ct);
        if (tenant is null)
            return Result.Failure("TENANT_NOT_FOUND", "Tenant not found.");

        _repository.Delete(tenant);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
