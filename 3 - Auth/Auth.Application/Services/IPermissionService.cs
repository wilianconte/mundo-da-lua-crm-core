namespace MyCRM.Auth.Application.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    void InvalidateCache(Guid userId);
}
