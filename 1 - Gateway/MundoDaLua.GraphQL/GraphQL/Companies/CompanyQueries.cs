using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Companies;

[QueryType]
public sealed class CompanyQueries
{
    [Authorize(Policy = SystemPermissions.CompaniesRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Company> GetCompanies([Service] CRMDbContext db) =>
        db.Companies.AsNoTracking();

    [Authorize(Policy = SystemPermissions.CompaniesRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Company> GetCompanyById(Guid id, [Service] CRMDbContext db) =>
        db.Companies.AsNoTracking().Where(x => x.Id == id);
}
