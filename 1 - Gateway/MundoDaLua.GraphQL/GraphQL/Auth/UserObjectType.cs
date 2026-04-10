using MyCRM.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Infrastructure.Persistence;

namespace MyCRM.GraphQL.GraphQL.Auth;

public sealed class UserObjectType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Name("User");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Email);
        descriptor.Field(x => x.IsActive);
        descriptor.Field(x => x.IsAdmin);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Field(x => x.CreatedBy);
        descriptor.Field(x => x.UpdatedBy);
        descriptor.Field(x => x.PersonId);
        descriptor
            .Field("roles")
            .ResolveWith<Resolvers>(x => x.GetRolesAsync(default!, default!, default))
            .Type<NonNullType<ListType<NonNullType<ObjectType<Role>>>>>();

        descriptor.Ignore(x => x.PasswordHash);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.UserRoles);
    }

    private sealed class Resolvers
    {
        public async Task<IReadOnlyList<Role>> GetRolesAsync(
            [Parent] User user,
            [Service] AuthDbContext db,
            CancellationToken ct)
        {
            return await db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role)
                .ToListAsync(ct);
        }
    }
}
