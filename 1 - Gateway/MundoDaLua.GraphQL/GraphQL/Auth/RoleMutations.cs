using MediatR;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Application.Commands.Roles.DeleteRole;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;
using MyCRM.GraphQL.GraphQL.Auth.Payloads;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[MutationType]
public sealed class RoleMutations
{
    public async Task<RolePayload> CreateRoleAsync(
        CreateRoleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new CreateRoleCommand(input.Name, input.Description, input.Permissions);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? new RolePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<RolePayload> UpdateRoleAsync(
        Guid id,
        UpdateRoleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateRoleCommand(id, input.Name, input.Description, input.Permissions);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? new RolePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<bool> DeleteRoleAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteRoleCommand(id);
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
