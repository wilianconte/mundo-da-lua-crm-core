using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentTaskObjectType : ObjectType<AppointmentTask>
{
    protected override void Configure(IObjectTypeDescriptor<AppointmentTask> descriptor)
    {
        descriptor.Name("AppointmentTask");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.AppointmentId);
        descriptor.Field(x => x.Type);
        descriptor.Field(x => x.AssignedToUserId);
        descriptor.Field(x => x.AssignedToRole);
        descriptor.Field(x => x.Status);
        descriptor.Field(x => x.Result);
        descriptor.Field(x => x.ResolvedAt);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);

        descriptor
            .Field("appointment")
            .Type<ObjectType<Appointment>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var task = context.Parent<AppointmentTask>();
                return await db.Appointments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == task.AppointmentId, context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Appointment);
    }
}
