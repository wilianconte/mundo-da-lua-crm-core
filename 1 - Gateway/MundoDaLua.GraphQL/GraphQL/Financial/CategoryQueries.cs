using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[QueryType]
public sealed class CategoryQueries
{
    [Authorize(Policy = SystemPermissions.CategoriesRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> GetCategories([Service] CRMDbContext db) =>
        db.FinancialCategories.AsNoTracking();
}
