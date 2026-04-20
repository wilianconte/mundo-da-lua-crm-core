using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.PaymentMethods.CreatePaymentMethod;
using MyCRM.CRM.Application.Commands.PaymentMethods.UpdatePaymentMethod;
using MyCRM.CRM.Application.Commands.PaymentMethods.DeletePaymentMethod;
using MyCRM.GraphQL.GraphQL.Financial.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[MutationType]
public sealed class PaymentMethodMutations
{
    [Authorize(Policy = SystemPermissions.PaymentMethodsCreate)]
    public async Task<PaymentMethodPayload> CreatePaymentMethodAsync(
        CreatePaymentMethodInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreatePaymentMethodCommand(input.Name, input.WalletId), ct);
        return result.IsSuccess
            ? new PaymentMethodPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.PaymentMethodsUpdate)]
    public async Task<PaymentMethodPayload> UpdatePaymentMethodAsync(
        Guid id,
        UpdatePaymentMethodInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePaymentMethodCommand(id, input.Name), ct);
        return result.IsSuccess
            ? new PaymentMethodPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.PaymentMethodsDelete)]
    public async Task<bool> DeletePaymentMethodAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeletePaymentMethodCommand(id), ct);
        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
