using MediatR;
using MyCRM.Auth.Application.Commands.Tenants.RegisterTenant;
using MyCRM.GraphQL.GraphQL.Tenants.Inputs;

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
}
