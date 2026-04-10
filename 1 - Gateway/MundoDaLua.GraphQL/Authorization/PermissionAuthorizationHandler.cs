using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MyCRM.Auth.Application.Services;

namespace MyCRM.GraphQL.Authorization;

public record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
        => _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub");

        if (userId is null || !Guid.TryParse(userId, out var userGuid))
            return;

        var isAdmin = context.User.FindFirstValue("is_admin") == "true";
        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        var has = await _permissionService.HasPermissionAsync(userGuid, requirement.Permission);
        if (has)
            context.Succeed(requirement);
    }
}
