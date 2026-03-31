using MyCRM.Auth.Domain.Entities;

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
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.UserRoles);
    }
}
