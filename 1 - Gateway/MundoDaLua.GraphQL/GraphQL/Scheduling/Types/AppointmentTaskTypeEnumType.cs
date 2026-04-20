using HotChocolate.Types;
using DomAppointmentTaskType = MyCRM.CRM.Domain.Entities.AppointmentTaskType;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentTaskTypeEnumType : EnumType<DomAppointmentTaskType>
{
    protected override void Configure(IEnumTypeDescriptor<DomAppointmentTaskType> descriptor)
    {
        descriptor.Name("AppointmentTaskType");
        descriptor.Value(DomAppointmentTaskType.NoShowDecision).Name("NO_SHOW_DECISION");
        descriptor.Value(DomAppointmentTaskType.Escalation).Name("ESCALATION");
    }
}
