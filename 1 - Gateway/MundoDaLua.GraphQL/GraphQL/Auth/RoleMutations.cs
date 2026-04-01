using MediatR;
using MyCRM.Auth.Application.Commands.Roles.CreateRole;
using MyCRM.Auth.Application.Commands.Roles.DeleteRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRole;
using MyCRM.Auth.Application.Commands.Roles.UpdateRolePermissions;
using MyCRM.Auth.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Auth.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[MutationType]
public sealed class RoleMutations
{
    [Authorize(Policy = SystemPermissions.RolesManage)]
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

    [Authorize(Policy = SystemPermissions.RolesManage)]
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

    [Authorize(Policy = SystemPermissions.RolesManage)]
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

    [Authorize(Policy = SystemPermissions.RolesManage)]
    public async Task<RoleDto> UpdateRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<Guid> permissionIds,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateRolePermissionsCommand(roleId, permissionIds), ct);

        return result.IsSuccess
            ? result.Value!
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}

