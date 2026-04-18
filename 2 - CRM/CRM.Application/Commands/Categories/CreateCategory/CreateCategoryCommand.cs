using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.CreateCategory;

public record CreateCategoryCommand(string Name) : IRequest<Result<CategoryDto>>;
