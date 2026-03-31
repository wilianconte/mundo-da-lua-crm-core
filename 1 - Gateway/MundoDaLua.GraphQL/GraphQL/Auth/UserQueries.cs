using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[QueryType]
public sealed class UserQueries
{
    [Authorize(Policy = SystemPermissions.UsersManage)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AuthDbContext db) =>
        db.Users.AsNoTracking();

    [Authorize(Policy = SystemPermissions.UsersManage)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<User> GetUserById(Guid id, [Service] AuthDbContext db) =>
        db.Users.AsNoTracking().Where(x => x.Id == id);
}
