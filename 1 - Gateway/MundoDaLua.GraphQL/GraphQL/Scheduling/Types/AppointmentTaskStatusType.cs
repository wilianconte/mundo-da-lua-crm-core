using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentTaskStatusType : EnumType<AppointmentTaskStatus>
{
    protected override void Configure(IEnumTypeDescriptor<AppointmentTaskStatus> descriptor)
    {
        descriptor.Name("AppointmentTaskStatus");
        descriptor.Value(AppointmentTaskStatus.Pending).Name("PENDING");
        descriptor.Value(AppointmentTaskStatus.Completed).Name("COMPLETED");
        descriptor.Value(AppointmentTaskStatus.Escalated).Name("ESCALATED");
        descriptor.Value(AppointmentTaskStatus.Expired).Name("EXPIRED");
    }
}
