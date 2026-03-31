using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Queries.GetMyPermissions;
using MyCRM.Auth.Application.Queries.GetPermissions;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[QueryType]
public sealed class PermissionQueries
{
    public async Task<IReadOnlyList<string>> GetMyPermissionsAsync(
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetMyPermissionsQuery(), ct);
        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.RolesManage)]
    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPermissionsQuery(), ct);
        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
