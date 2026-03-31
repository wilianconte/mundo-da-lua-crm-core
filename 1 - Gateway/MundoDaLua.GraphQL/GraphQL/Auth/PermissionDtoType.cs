using MyCRM.Auth.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Auth;

public sealed class PermissionDtoType : ObjectType<PermissionDto>
{
    protected override void Configure(IObjectTypeDescriptor<PermissionDto> descriptor)
    {
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Group);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.IsActive);
    }
}
