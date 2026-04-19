using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.CreateCategory;

public sealed class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly ITenantService      _tenant;

    public CreateCategoryHandler(ICategoryRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = Category.Create(_tenant.TenantId, request.Name);
        await _repository.AddAsync(category, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<CategoryDto>.Success(category.Adapt<CategoryDto>());
    }
}
