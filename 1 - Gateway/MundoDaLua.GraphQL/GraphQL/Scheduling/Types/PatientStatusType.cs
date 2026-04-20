using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class PatientStatusType : EnumType<PatientStatus>
{
    protected override void Configure(IEnumTypeDescriptor<PatientStatus> descriptor)
    {
        descriptor.Name("PatientStatus");
        descriptor.Value(PatientStatus.Active).Name("ACTIVE");
        descriptor.Value(PatientStatus.Inactive).Name("INACTIVE");
        descriptor.Value(PatientStatus.Blocked).Name("BLOCKED");
    }
}
