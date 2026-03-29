using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.GraphQL.GraphQL.Auth;

[Authorize]
[QueryType]
public sealed class RoleQueries
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Role> GetRoles([Service] AuthDbContext db) =>
        db.Roles.AsNoTracking();

    public async Task<Role?> GetRoleByIdAsync(
        Guid id,
        [Service] AuthDbContext db,
        CancellationToken ct) =>
        await db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
}
