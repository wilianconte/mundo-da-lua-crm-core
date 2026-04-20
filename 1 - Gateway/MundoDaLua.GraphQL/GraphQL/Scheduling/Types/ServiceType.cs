using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ServiceType : ObjectType<Service>
{
    protected override void Configure(IObjectTypeDescriptor<Service> descriptor)
    {
        descriptor.Name("Service");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.DefaultPrice);
        descriptor.Field(x => x.DefaultDurationInMinutes);
        descriptor.Field(x => x.IsActive);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
