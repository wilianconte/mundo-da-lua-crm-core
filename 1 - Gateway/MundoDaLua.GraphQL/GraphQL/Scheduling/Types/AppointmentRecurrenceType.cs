using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentRecurrenceType : ObjectType<AppointmentRecurrence>
{
    protected override void Configure(IObjectTypeDescriptor<AppointmentRecurrence> descriptor)
    {
        descriptor.Name("AppointmentRecurrence");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ParentAppointmentId);
        descriptor.Field(x => x.Frequency);
        descriptor.Field(x => x.EndDate);
        descriptor.Field(x => x.MaxOccurrences);
        descriptor.Field(x => x.CreatedOccurrences);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
