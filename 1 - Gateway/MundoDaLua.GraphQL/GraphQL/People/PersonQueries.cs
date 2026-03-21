using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.People;

[QueryType]
public sealed class PersonQueries
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPeople([Service] CRMDbContext db) =>
        db.People.AsNoTracking();

    [Authorize]
    public async Task<Person?> GetPersonByIdAsync(
        Guid id,
        [Service] CRMDbContext db,
        CancellationToken ct) =>
        await db.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
}
