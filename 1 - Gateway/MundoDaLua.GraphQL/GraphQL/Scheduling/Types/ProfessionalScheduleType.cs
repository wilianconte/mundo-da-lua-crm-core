using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalScheduleType : ObjectType<ProfessionalSchedule>
{
    protected override void Configure(IObjectTypeDescriptor<ProfessionalSchedule> descriptor)
    {
        descriptor.Name("ProfessionalSchedule");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.DayOfWeek);
        descriptor.Field(x => x.StartTime);
        descriptor.Field(x => x.EndTime);
        descriptor.Field(x => x.IsAvailable);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Professional);
    }
}
