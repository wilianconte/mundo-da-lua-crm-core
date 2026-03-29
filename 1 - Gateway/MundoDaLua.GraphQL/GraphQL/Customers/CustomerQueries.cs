using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Customers;

[Authorize]
[QueryType]
public class CustomerQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers([Service] CRMDbContext db) =>
        db.Customers.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Customer> GetCustomerById(Guid id, [Service] CRMDbContext db) =>
        db.Customers.AsNoTracking().Where(x => x.Id == id);
}
