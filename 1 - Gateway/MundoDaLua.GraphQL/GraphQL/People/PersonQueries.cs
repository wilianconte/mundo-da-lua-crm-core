using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.People;

[Authorize]
[QueryType]
public sealed class PersonQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPeople([Service] CRMDbContext db) =>
        db.People.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Person> GetPersonById(Guid id, [Service] CRMDbContext db) =>
        db.People.AsNoTracking().Where(x => x.Id == id);
}
