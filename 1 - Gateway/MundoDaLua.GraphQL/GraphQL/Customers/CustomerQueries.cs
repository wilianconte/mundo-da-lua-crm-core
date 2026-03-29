using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Customers;

[QueryType]
public class CustomerQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers(
        [Service] CRMDbContext db,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Não autorizado. Faça login para continuar.")
                    .SetCode("AUTH_NOT_AUTHORIZED")
                    .Build());

        return db.Customers.AsNoTracking();
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Customer> GetCustomerById(
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

        return db.Customers.AsNoTracking().Where(x => x.Id == id);
    }
}
