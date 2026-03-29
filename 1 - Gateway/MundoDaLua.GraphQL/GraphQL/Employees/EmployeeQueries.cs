using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Employees;

[Authorize]
[QueryType]
public sealed class EmployeeQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Employee> GetEmployees([Service] CRMDbContext db) =>
        db.Employees.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Employee> GetEmployeeById(Guid id, [Service] CRMDbContext db) =>
        db.Employees.AsNoTracking().Where(x => x.Id == id);
}
