using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.People;

[QueryType]
public sealed class PersonQueries
{
    [Authorize(Policy = SystemPermissions.PeopleRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPeople([Service] CRMDbContext db) =>
        db.People.AsNoTracking();

    [Authorize(Policy = SystemPermissions.PeopleRead)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Person> GetPersonById(Guid id, [Service] CRMDbContext db) =>
        db.People.AsNoTracking().Where(x => x.Id == id);
}
