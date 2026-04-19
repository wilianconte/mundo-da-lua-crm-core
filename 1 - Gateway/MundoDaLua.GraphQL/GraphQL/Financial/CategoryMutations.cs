using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Categories.CreateCategory;
using MyCRM.CRM.Application.Commands.Categories.UpdateCategory;
using MyCRM.CRM.Application.Commands.Categories.DeleteCategory;
using MyCRM.GraphQL.GraphQL.Financial.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[MutationType]
public sealed class CategoryMutations
{
    [Authorize(Policy = SystemPermissions.CategoriesCreate)]
    public async Task<CategoryPayload> CreateCategoryAsync(
        CreateCategoryInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateCategoryCommand(input.Name), ct);
        return result.IsSuccess
            ? new CategoryPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CategoriesUpdate)]
    public async Task<CategoryPayload> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCategoryCommand(id, input.Name), ct);
        return result.IsSuccess
            ? new CategoryPayload(result.Value!)
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    [Authorize(Policy = SystemPermissions.CategoriesDelete)]
    public async Task<bool> DeleteCategoryAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), ct);
        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
