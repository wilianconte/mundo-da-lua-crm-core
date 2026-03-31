using MediatR;
using MyCRM.Auth.Application.Services;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Queries.GetMyPermissions;

public sealed class GetMyPermissionsHandler : IRequestHandler<GetMyPermissionsQuery, Result<IReadOnlyList<string>>>
{
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUserService _currentUser;

    public GetMyPermissionsHandler(IPermissionService permissionService, ICurrentUserService currentUser)
    {
        _permissionService = permissionService;
        _currentUser       = currentUser;
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(GetMyPermissionsQuery request, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            return Result<IReadOnlyList<string>>.Failure("UNAUTHORIZED", "User not authenticated.");

        var permissions = await _permissionService.GetUserPermissionsAsync(_currentUser.UserId.Value, ct);
        return Result<IReadOnlyList<string>>.Success(permissions);
    }
}
