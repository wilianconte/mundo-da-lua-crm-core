using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Transactions.CreateTransaction;
using MyCRM.CRM.Application.Commands.Transactions.UpdateTransaction;
using MyCRM.CRM.Application.Commands.Transactions.DeleteTransaction;
using MyCRM.CRM.Application.Commands.Transactions.ReconcileTransaction;
using MyCRM.CRM.Application.Commands.Transactions.CreateTransfer;
using MyCRM.GraphQL.GraphQL.Financial.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[MutationType]
public sealed class TransactionMutations
{
    [Authorize(Policy = SystemPermissions.TransactionsCreate)]
    public async Task<TransactionPayload> CreateTransactionAsync(
        CreateTransactionInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateTransactionCommand(
            input.WalletId, input.Type, input.Amount, input.Description,
            input.CategoryId, input.PaymentMethodId, input.TransactionDate), ct);
        return result.IsSuccess
            ? new TransactionPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.TransactionsUpdate)]
    public async Task<TransactionPayload> UpdateTransactionAsync(
        Guid id,
        UpdateTransactionInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateTransactionCommand(
            id, input.Amount, input.Description,
            input.CategoryId, input.PaymentMethodId, input.TransactionDate), ct);
        return result.IsSuccess
            ? new TransactionPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.TransactionsDelete)]
    public async Task<bool> DeleteTransactionAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteTransactionCommand(id), ct);
        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.TransactionsReconcile)]
    public async Task<ReconciliationPayload> ReconcileTransactionAsync(
        ReconcileTransactionInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ReconcileTransactionCommand(
            input.TransactionId, input.ExternalId, input.ExternalAmount, input.ExternalDate), ct);
        return result.IsSuccess
            ? new ReconciliationPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.TransactionsTransfer)]
    public async Task<TransferPayload> CreateTransferAsync(
        CreateTransferInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateTransferCommand(
            input.FromWalletId, input.ToWalletId, input.Amount, input.Description,
            input.CategoryId, input.PaymentMethodId, input.TransactionDate), ct);
        return result.IsSuccess
            ? new TransferPayload(result.Value!.ExpenseTransaction, result.Value!.IncomeTransaction)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
