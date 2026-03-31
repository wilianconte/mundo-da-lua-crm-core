using MediatR;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _repository;
    private readonly ITenantService _tenant;

    public UpdateRoleHandler(IRoleRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await _repository.GetByIdAsync(request.Id, ct);

        if (role is null || role.TenantId != _tenant.TenantId)
            return Result<RoleDto>.Failure("ROLE_NOT_FOUND", "Role not found.");

        var nameExists = await _repository.NameExistsAsync(_tenant.TenantId, request.Name, excludeId: request.Id, ct);
        if (nameExists)
            return Result<RoleDto>.Failure("ROLE_NAME_DUPLICATE", "A role with this name already exists.");

        role.Update(request.Name, request.Description);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) role.Activate();
            else role.Deactivate();
        }

        _repository.Update(role);
        await _repository.SaveChangesAsync(ct);

        return Result<RoleDto>.Success(CreateRoleHandler.ToDto(role));
    }
}
