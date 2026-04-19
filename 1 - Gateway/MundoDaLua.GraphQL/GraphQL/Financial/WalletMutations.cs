using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Wallets.CreateWallet;
using MyCRM.CRM.Application.Commands.Wallets.UpdateWallet;
using MyCRM.CRM.Application.Commands.Wallets.DeleteWallet;
using MyCRM.GraphQL.GraphQL.Financial.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[MutationType]
public sealed class WalletMutations
{
    [Authorize(Policy = SystemPermissions.WalletsCreate)]
    public async Task<WalletPayload> CreateWalletAsync(
        CreateWalletInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateWalletCommand(input.Name, input.InitialBalance), ct);
        return result.IsSuccess
            ? new WalletPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.WalletsUpdate)]
    public async Task<WalletPayload> UpdateWalletAsync(
        Guid id,
        UpdateWalletInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateWalletCommand(id, input.Name, input.InitialBalance), ct);
        return result.IsSuccess
            ? new WalletPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.WalletsDelete)]
    public async Task<bool> DeleteWalletAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteWalletCommand(id), ct);
        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
