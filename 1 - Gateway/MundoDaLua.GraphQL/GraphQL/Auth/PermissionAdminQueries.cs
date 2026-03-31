using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Queries.GetPermissions;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize(Policy = SystemPermissions.RolesManage)]
[QueryType]
public sealed class PermissionAdminQueries
{
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
