using MyCRM.Auth.Domain.Entities;

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
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Field(x => x.CreatedBy);
        descriptor.Field(x => x.UpdatedBy);
        descriptor.Field(x => x.PersonId);

        descriptor.Ignore(x => x.PasswordHash);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
