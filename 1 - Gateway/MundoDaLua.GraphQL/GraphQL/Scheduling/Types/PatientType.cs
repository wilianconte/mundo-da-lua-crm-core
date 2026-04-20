using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class PatientType : ObjectType<Patient>
{
    protected override void Configure(IObjectTypeDescriptor<Patient> descriptor)
    {
        descriptor.Name("Patient");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.PersonId);
        descriptor.Field(x => x.Status);
        descriptor.Field(x => x.Notes);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);

        descriptor
            .Field("person")
            .Type<ObjectType<Person>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var patient = context.Parent<Patient>();
                return await db.People
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == patient.PersonId, context.RequestAborted);
            });

        descriptor
            .Field("appointments")
            .Type<NonNullType<ListType<NonNullType<ObjectType<Appointment>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var patient = context.Parent<Patient>();
                return await db.Appointments
                    .AsNoTracking()
                    .Where(a => a.PatientId == patient.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Person);
    }
}
