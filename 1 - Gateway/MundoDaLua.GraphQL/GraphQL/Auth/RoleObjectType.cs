using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Auth;

public sealed class RoleObjectType : ObjectType<Role>
{
    protected override void Configure(IObjectTypeDescriptor<Role> descriptor)
    {
        descriptor.Name("Role");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.IsActive);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Field(x => x.CreatedBy);
        descriptor.Field(x => x.UpdatedBy);
        descriptor
            .Field("permissions")
            .Resolve(async context =>
            {
                var db = context.Service<AuthDbContext>();
                var role = context.Parent<Role>();

                return await db.RolePermissions
                    .AsNoTracking()
                    .Where(rp => rp.RoleId == role.Id && rp.Permission.IsActive)
                    .Select(rp => new PermissionDto(
                        rp.Permission.Id,
                        rp.Permission.Name,
                        rp.Permission.Group,
                        rp.Permission.Description,
                        rp.Permission.IsActive))
                    .ToListAsync(context.RequestAborted);
            });
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.UserRoles);
    }
}
