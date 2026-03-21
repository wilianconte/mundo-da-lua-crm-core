using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Customers;

public sealed class CustomerObjectType : ObjectType<Customer>
{
    protected override void Configure(IObjectTypeDescriptor<Customer> descriptor)
    {
        descriptor.Name("Customer");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Email);
        descriptor.Field(x => x.Phone);
        descriptor.Field(x => x.Document);
        descriptor.Field(x => x.Type);
        descriptor.Field(x => x.Status);
        descriptor.Field(x => x.Address);
        descriptor.Field(x => x.Notes);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
