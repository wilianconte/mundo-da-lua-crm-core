using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalSpecialtyType : ObjectType<ProfessionalSpecialty>
{
    protected override void Configure(IObjectTypeDescriptor<ProfessionalSpecialty> descriptor)
    {
        descriptor.Name("ProfessionalSpecialty");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
