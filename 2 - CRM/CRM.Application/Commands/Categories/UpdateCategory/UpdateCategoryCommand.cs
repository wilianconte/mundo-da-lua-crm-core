using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result<CategoryDto>>;
