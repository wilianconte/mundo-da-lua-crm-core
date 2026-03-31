using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize(Policy = SystemPermissions.RolesManage)]
[QueryType]
public sealed class RoleQueries
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Role> GetRoles([Service] AuthDbContext db) =>
        db.Roles.AsNoTracking();

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Role> GetRoleById(Guid id, [Service] AuthDbContext db) =>
        db.Roles.AsNoTracking().Where(x => x.Id == id);
}
