using Microsoft.Extensions.Caching.Memory;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;

namespace MyCRM.Auth.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private static string CacheKey(Guid userId) => $"perms:{userId}";

    public PermissionService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IMemoryCache cache)
    {
        _userRepository       = userRepository;
        _permissionRepository = permissionRepository;
        _cache                = cache;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey(userId), out IReadOnlyList<string>? cached))
            return cached!;

        var user = await _userRepository.GetByIdWithRolesAsync(userId, ct);
        var roleIds = user?.UserRoles.Select(r => r.RoleId).ToList() ?? [];
        var permissions = await _permissionRepository.GetPermissionNamesByRoleIdsAsync(roleIds, ct);

        _cache.Set(CacheKey(userId), permissions, CacheTtl);
        return permissions;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        var permissions = await GetUserPermissionsAsync(userId, ct);
        return permissions.Contains(permission);
    }

    public void InvalidateCache(Guid userId) =>
        _cache.Remove(CacheKey(userId));
}
