using MediatR;
using MyCRM.Auth.Application.Commands.UserRoles.AssignRoleToUser;
using MyCRM.Auth.Application.Commands.UserRoles.RemoveRoleFromUser;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[MutationType]
public sealed class UserRoleMutations
{
    public async Task<bool> AssignRoleToUserAsync(
        AssignRoleToUserInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new AssignRoleToUserCommand(input.UserId, input.RoleId);
        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));

        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(
        RemoveRoleFromUserInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new RemoveRoleFromUserCommand(input.UserId, input.RoleId);
        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));

        return true;
    }
}
