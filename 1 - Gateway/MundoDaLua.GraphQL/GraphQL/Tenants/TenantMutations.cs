using MediatR;
using MyCRM.Auth.Application.Commands.Tenants.DeleteTenant;
using MyCRM.Auth.Application.Commands.Tenants.RegisterTenant;
using MyCRM.Auth.Application.Commands.Tenants.UpdateTenant;
using MyCRM.GraphQL.GraphQL.Tenants.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Tenants;

[MutationType]
public sealed class TenantMutations
{
    /// <summary>
    /// Registra um novo tenant criando empresa, pessoa administradora e acesso em uma única operação.
    /// Mutation pública — não requer autenticação.
    /// </summary>
    [AllowAnonymous]
    public async Task<TenantPayload> RegisterTenantAsync(
        RegisterTenantInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RegisterTenantCommand(
            input.CompanyLegalName,
            input.CompanyCnpj,
            input.CompanyEmail,
            input.CompanyPhone,
            input.AdminName,
            input.AdminEmail,
            input.AdminCpf,
            input.AdminPhone,
            input.Password,
            input.PasswordConfirmation), ct);

        return result.IsSuccess
            ? new TenantPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = SystemPermissions.TenantsManage)]
    public async Task<TenantPayload> UpdateTenantAsync(
        Guid id,
        UpdateTenantInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateTenantCommand(id, input.Name, input.Status), ct);

        return result.IsSuccess
            ? new TenantPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.TenantsManage)]
    public async Task<bool> DeleteTenantAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTenantCommand(id), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
