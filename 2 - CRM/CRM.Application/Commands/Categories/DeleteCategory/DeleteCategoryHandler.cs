using MediatR;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.DeleteCategory;

public sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result.Failure("CATEGORY_NOT_FOUND", "Category not found.");

        category.SoftDelete();
        _repository.Update(category);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
