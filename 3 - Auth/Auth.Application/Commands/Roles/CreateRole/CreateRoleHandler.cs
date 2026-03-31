using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.CreateRole;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _repository;
    private readonly ITenantService _tenant;

    public CreateRoleHandler(IRoleRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var nameExists = await _repository.NameExistsAsync(_tenant.TenantId, request.Name, excludeId: null, ct);
        if (nameExists)
            return Result<RoleDto>.Failure("ROLE_NAME_DUPLICATE", "A role with this name already exists.");

        var role = Role.Create(_tenant.TenantId, request.Name, request.Description);

        await _repository.AddAsync(role, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<RoleDto>.Success(ToDto(role));
    }

    internal static RoleDto ToDto(Role role) => new(
        role.Id,
        role.TenantId,
        role.Name,
        role.Description,
        role.IsActive,
        role.RolePermissions
            .Select(rp => new PermissionDto(rp.PermissionId, rp.Permission?.Name ?? string.Empty, rp.Permission?.Group ?? string.Empty, rp.Permission?.Description, rp.Permission?.IsActive ?? false))
            .ToList(),
        role.CreatedAt,
        role.UpdatedAt,
        role.CreatedBy,
        role.UpdatedBy);
}
