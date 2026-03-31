using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Customers;

[QueryType]
public class CustomerQueries
{
    [Authorize(Policy = SystemPermissions.CustomersRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers([Service] CRMDbContext db) =>
        db.Customers.AsNoTracking();

    [Authorize(Policy = SystemPermissions.CustomersRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Customer> GetCustomerById(Guid id, [Service] CRMDbContext db) =>
        db.Customers.AsNoTracking().Where(x => x.Id == id);
}
