using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
