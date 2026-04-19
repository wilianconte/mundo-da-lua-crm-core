using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;
