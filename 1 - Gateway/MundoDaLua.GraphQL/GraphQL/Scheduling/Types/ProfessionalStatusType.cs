using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalStatusType : EnumType<ProfessionalStatus>
{
    protected override void Configure(IEnumTypeDescriptor<ProfessionalStatus> descriptor)
    {
        descriptor.Name("ProfessionalStatus");
        descriptor.Value(ProfessionalStatus.Draft).Name("DRAFT");
        descriptor.Value(ProfessionalStatus.Active).Name("ACTIVE");
        descriptor.Value(ProfessionalStatus.Inactive).Name("INACTIVE");
        descriptor.Value(ProfessionalStatus.Suspended).Name("SUSPENDED");
        descriptor.Value(ProfessionalStatus.Terminated).Name("TERMINATED");
    }
}
