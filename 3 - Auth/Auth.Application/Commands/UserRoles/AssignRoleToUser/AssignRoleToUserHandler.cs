using MediatR;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.UserRoles.AssignRoleToUser;

public sealed class AssignRoleToUserHandler : IRequestHandler<AssignRoleToUserCommand, Result>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantService _tenantService;

    public AssignRoleToUserHandler(
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ITenantService tenantService)
    {
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tenantService = tenantService;
    }

    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken ct)
    {
        var tenantId = _tenantService.TenantId;

        // Validar existência do usuário
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure("USER_NOT_FOUND", "User not found.");

        // Validar existência da role
        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);
        if (role is null)
            return Result.Failure("ROLE_NOT_FOUND", "Role not found.");

        // Verificar se o usuário já possui essa role
        if (await _userRoleRepository.UserHasRoleAsync(tenantId, request.UserId, request.RoleId, ct))
            return Result.Failure("USER_ROLE_DUPLICATE", "User already has this role.");

        // Criar associação
        var userRole = UserRole.Create(request.UserId, request.RoleId);

        await _userRoleRepository.AddAsync(userRole, ct);
        await _userRoleRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
