using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Auth;

[QueryType]
public sealed class RoleQueries
{
    [Authorize(Policy = SystemPermissions.RolesManage)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Role> GetRoles([Service] AuthDbContext db) =>
        db.Roles.AsNoTracking();

    [Authorize(Policy = SystemPermissions.RolesManage)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Role> GetRoleById(Guid id, [Service] AuthDbContext db) =>
        db.Roles.AsNoTracking().Where(x => x.Id == id);
}
