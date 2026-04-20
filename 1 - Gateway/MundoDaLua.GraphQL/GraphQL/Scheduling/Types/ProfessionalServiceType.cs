using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalServiceType : ObjectType<ProfessionalService>
{
    protected override void Configure(IObjectTypeDescriptor<ProfessionalService> descriptor)
    {
        descriptor.Name("ProfessionalService");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.ServiceId);
        descriptor.Field(x => x.CustomPrice);
        descriptor.Field(x => x.CustomDurationInMinutes);
        descriptor.Field(x => x.IsActive);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);

        descriptor
            .Field("professional")
            .Type<ObjectType<Professional>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var ps = context.Parent<ProfessionalService>();
                return await db.Professionals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == ps.ProfessionalId, context.RequestAborted);
            });

        descriptor
            .Field("service")
            .Type<ObjectType<Service>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var ps = context.Parent<ProfessionalService>();
                return await db.Services
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == ps.ServiceId, context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Professional);
        descriptor.Ignore(x => x.Service);
    }
}
