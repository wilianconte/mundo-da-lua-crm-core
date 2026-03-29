using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Companies;

[Authorize]
[QueryType]
public sealed class CompanyQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Company> GetCompanies([Service] CRMDbContext db) =>
        db.Companies.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Company> GetCompanyById(Guid id, [Service] CRMDbContext db) =>
        db.Companies.AsNoTracking().Where(x => x.Id == id);
}
