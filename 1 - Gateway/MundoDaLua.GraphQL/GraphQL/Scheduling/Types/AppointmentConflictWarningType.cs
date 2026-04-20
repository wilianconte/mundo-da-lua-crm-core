using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentConflictWarningType : ObjectType<AppointmentDto>
{
    protected override void Configure(IObjectTypeDescriptor<AppointmentDto> descriptor)
    {
        descriptor.Name("AppointmentConflictWarning");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.PatientId);
        descriptor.Field(x => x.ServiceId);
        descriptor.Field(x => x.StartDateTime);
        descriptor.Field(x => x.EndDateTime);
        descriptor.Field(x => x.Status);
    }
}
