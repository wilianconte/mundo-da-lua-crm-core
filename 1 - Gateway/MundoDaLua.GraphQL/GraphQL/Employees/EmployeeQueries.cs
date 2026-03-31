using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Employees;

[QueryType]
public sealed class EmployeeQueries
{
    [Authorize(Policy = SystemPermissions.EmployeesRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Employee> GetEmployees([Service] CRMDbContext db) =>
        db.Employees.AsNoTracking();

    [Authorize(Policy = SystemPermissions.EmployeesRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Employee> GetEmployeeById(Guid id, [Service] CRMDbContext db) =>
        db.Employees.AsNoTracking().Where(x => x.Id == id);
}
