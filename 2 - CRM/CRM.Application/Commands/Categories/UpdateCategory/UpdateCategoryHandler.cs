using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result<CategoryDto>.Failure("CATEGORY_NOT_FOUND", "Category not found.");

        category.Update(request.Name);
        _repository.Update(category);
        await _repository.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(category.Adapt<CategoryDto>());
    }
}
