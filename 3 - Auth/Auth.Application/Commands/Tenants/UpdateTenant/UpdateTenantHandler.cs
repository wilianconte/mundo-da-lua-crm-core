using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Tenants.UpdateTenant;

public sealed class UpdateTenantHandler : IRequestHandler<UpdateTenantCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _repository;

    public UpdateTenantHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TenantDto>> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _repository.GetByIdAsync(request.Id, ct);
        if (tenant is null)
            return Result<TenantDto>.Failure("TENANT_NOT_FOUND", "Tenant not found.");

        var nameExists = await _repository.NameExistsAsync(request.Name, excludeId: request.Id, ct);
        if (nameExists)
            return Result<TenantDto>.Failure("TENANT_NAME_DUPLICATE", "A tenant with this name already exists.");

        tenant.UpdateName(request.Name);

        switch (request.Status)
        {
            case TenantStatus.Active:    tenant.Activate();  break;
            case TenantStatus.Suspended: tenant.Suspend();   break;
            case TenantStatus.Cancelled: tenant.Cancel();    break;
        }

        _repository.Update(tenant);
        await _repository.SaveChangesAsync(ct);

        return Result<TenantDto>.Success(ToDto(tenant));
    }

    internal static TenantDto ToDto(Tenant t) =>
        new(t.Id, t.Name, t.CompanyId, t.OwnerPersonId, t.Status, t.CreatedAt, t.UpdatedAt);
}
