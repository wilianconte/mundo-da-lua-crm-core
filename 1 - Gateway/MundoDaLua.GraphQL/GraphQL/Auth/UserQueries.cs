using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[QueryType]
public sealed class UserQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AuthDbContext db) =>
        db.Users.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<User> GetUserById(Guid id, [Service] AuthDbContext db) =>
        db.Users.AsNoTracking().Where(x => x.Id == id);
}
