using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class CommissionRuleType : ObjectType<CommissionRule>
{
    protected override void Configure(IObjectTypeDescriptor<CommissionRule> descriptor)
    {
        descriptor.Name("CommissionRule");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.ServiceId);
        descriptor.Field(x => x.CompanyPercentage);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Professional);
        descriptor.Ignore(x => x.Service);
    }
}
