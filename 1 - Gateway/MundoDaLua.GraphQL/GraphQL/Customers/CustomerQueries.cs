using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Customers;

[QueryType]
public class CustomerQueries
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers([Service] CRMDbContext db) =>
        db.Customers;

    [Authorize]
    public async Task<Customer?> GetCustomerByIdAsync(
        Guid id,
        [Service] CRMDbContext db,
        CancellationToken ct) =>
        await db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
}
