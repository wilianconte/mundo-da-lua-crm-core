using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.UpdateRolePermissions;

public sealed class UpdateRolePermissionsHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionService _permissionService;
    private readonly ITenantService _tenant;

    public UpdateRolePermissionsHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionService permissionService,
        ITenantService tenant)
    {
        _roleRepository       = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionService    = permissionService;
        _tenant               = tenant;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRolePermissionsCommand request, CancellationToken ct)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(request.RoleId, ct);

        if (role is null || role.TenantId != _tenant.TenantId)
            return Result<RoleDto>.Failure("ROLE_NOT_FOUND", "Role not found.");

        if (request.PermissionIds.Count > 0)
        {
            var allPermissions = await _permissionRepository.GetAllAsync(ct);
            var validIds = allPermissions.Select(p => p.Id).ToHashSet();
            var invalidIds = request.PermissionIds.Where(id => !validIds.Contains(id)).ToList();

            if (invalidIds.Count > 0)
                return Result<RoleDto>.Failure("PERMISSION_NOT_FOUND", "One or more permission IDs are invalid.");
        }

        var userIds = await _roleRepository.GetUserIdsByRoleIdAsync(request.RoleId, ct);

        role.SyncPermissions(request.PermissionIds);

        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(ct);

        foreach (var userId in userIds)
            _permissionService.InvalidateCache(userId);

        return Result<RoleDto>.Success(CreateRoleHandler.ToDto(role));
    }
}
