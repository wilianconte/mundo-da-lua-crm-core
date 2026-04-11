using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Tenants;

[QueryType]
public sealed class TenantQueries
{
    [Authorize(Policy = SystemPermissions.TenantsManage)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Tenant> GetTenants([Service] AuthDbContext db) =>
        db.Tenants.AsNoTracking();

    [Authorize(Policy = SystemPermissions.TenantsManage)]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Tenant> GetTenantById(Guid id, [Service] AuthDbContext db) =>
        db.Tenants.AsNoTracking().Where(x => x.Id == id);
}
