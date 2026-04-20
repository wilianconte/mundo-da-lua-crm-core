using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentStatusType : EnumType<AppointmentStatus>
{
    protected override void Configure(IEnumTypeDescriptor<AppointmentStatus> descriptor)
    {
        descriptor.Name("AppointmentStatus");
        descriptor.Value(AppointmentStatus.Suggested).Name("SUGGESTED");
        descriptor.Value(AppointmentStatus.Confirmed).Name("CONFIRMED");
        descriptor.Value(AppointmentStatus.Rescheduled).Name("RESCHEDULED");
        descriptor.Value(AppointmentStatus.Completed).Name("COMPLETED");
        descriptor.Value(AppointmentStatus.Cancelled).Name("CANCELLED");
        descriptor.Value(AppointmentStatus.NoShow).Name("NO_SHOW");
    }
}
