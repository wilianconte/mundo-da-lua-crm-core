using MediatR;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.UserRoles.RemoveRoleFromUser;

public sealed class RemoveRoleFromUserHandler : IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ITenantService _tenantService;

    public RemoveRoleFromUserHandler(IUserRoleRepository userRoleRepository, ITenantService tenantService)
    {
        _userRoleRepository = userRoleRepository;
        _tenantService = tenantService;
    }

    public async Task<Result> Handle(RemoveRoleFromUserCommand request, CancellationToken ct)
    {
        var tenantId = _tenantService.TenantId;

        var userRole = await _userRoleRepository.GetUserRoleAsync(tenantId, request.UserId, request.RoleId, ct);
        if (userRole is null)
            return Result.Failure("USER_ROLE_NOT_FOUND", "User does not have this role.");

        _userRoleRepository.Delete(userRole);
        await _userRoleRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
