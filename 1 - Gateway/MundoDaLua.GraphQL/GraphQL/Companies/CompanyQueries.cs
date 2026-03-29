using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Companies;

[QueryType]
public sealed class CompanyQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Company> GetCompanies(
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.Companies.AsNoTracking();
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Company> GetCompanyById(
        Guid id,
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.Companies.AsNoTracking().Where(x => x.Id == id);
    }
}
