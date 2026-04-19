using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCategories;

public sealed class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly ICategoryRepository _repository;

    public GetAllCategoriesHandler(ICategoryRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken ct)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<CategoryDto>>.Success(items.Adapt<IReadOnlyList<CategoryDto>>());
    }
}
