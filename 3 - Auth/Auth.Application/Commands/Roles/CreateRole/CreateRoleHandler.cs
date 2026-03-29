using Mapster;
using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.CreateRole;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantService _tenantService;

    public CreateRoleHandler(IRoleRepository roleRepository, ITenantService tenantService)
    {
        _roleRepository = roleRepository;
        _tenantService = tenantService;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var tenantId = _tenantService.TenantId;

        // Validação de duplicata
        if (await _roleRepository.NameExistsAsync(tenantId, request.Name, null, ct))
            return Result<RoleDto>.Failure("ROLE_NAME_DUPLICATE", "A role with this name already exists.");

        // Criar entidade via factory method
        var role = Role.Create(request.Name, request.Description, request.Permissions);

        // Persistir
        await _roleRepository.AddAsync(role, ct);
        await _roleRepository.SaveChangesAsync(ct);

        return Result<RoleDto>.Success(role.Adapt<RoleDto>());
    }
}
