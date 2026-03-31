using MediatR;
using MyCRM.Auth.Application.Queries.GetMyPermissions;

namespace MyCRM.GraphQL.GraphQL.Auth;

[QueryType]
public sealed class PermissionQueries
{
    [Authorize]
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
}
