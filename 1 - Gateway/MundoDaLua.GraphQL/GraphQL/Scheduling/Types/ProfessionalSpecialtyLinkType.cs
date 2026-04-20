using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalSpecialtyLinkType : ObjectType<ProfessionalSpecialtyLink>
{
    protected override void Configure(IObjectTypeDescriptor<ProfessionalSpecialtyLink> descriptor)
    {
        descriptor.Name("ProfessionalSpecialtyLink");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.SpecialtyId);

        descriptor
            .Field("specialty")
            .Type<ObjectType<ProfessionalSpecialty>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var link = context.Parent<ProfessionalSpecialtyLink>();
                return await db.ProfessionalSpecialties
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == link.SpecialtyId, context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
    }
}
