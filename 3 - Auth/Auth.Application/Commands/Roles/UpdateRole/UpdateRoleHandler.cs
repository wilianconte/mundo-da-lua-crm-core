using Mapster;
using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantService _tenantService;

    public UpdateRoleHandler(IRoleRepository roleRepository, ITenantService tenantService)
    {
        _roleRepository = roleRepository;
        _tenantService = tenantService;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var tenantId = _tenantService.TenantId;

        // Buscar role existente
        var role = await _roleRepository.GetByIdAsync(request.Id, ct);
        if (role is null)
            return Result<RoleDto>.Failure("ROLE_NOT_FOUND", "Role not found.");

        // Validação de duplicata de nome
        if (await _roleRepository.NameExistsAsync(tenantId, request.Name, request.Id, ct))
            return Result<RoleDto>.Failure("ROLE_NAME_DUPLICATE", "A role with this name already exists.");

        // Atualizar via método de domínio
        role.Update(request.Name, request.Description, request.Permissions);

        // Persistir
        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(ct);

        return Result<RoleDto>.Success(role.Adapt<RoleDto>());
    }
}
