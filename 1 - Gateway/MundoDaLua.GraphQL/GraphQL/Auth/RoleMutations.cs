using MediatR;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Commands.Roles.DeleteRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[MutationType]
public sealed class RoleMutations
{
    public async Task<RoleDto> CreateRoleAsync(
        CreateRoleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateRoleCommand(input.Name, input.Description), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    public async Task<RoleDto> UpdateRoleAsync(
        Guid id,
        UpdateRoleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateRoleCommand(id, input.Name, input.Description, input.IsActive), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    public async Task<bool> DeleteRoleAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRoleCommand(id), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
