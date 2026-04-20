using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class ProfessionalType : ObjectType<Professional>
{
    protected override void Configure(IObjectTypeDescriptor<Professional> descriptor)
    {
        descriptor.Name("Professional");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.PersonId);
        descriptor.Field(x => x.Status);
        descriptor.Field(x => x.Bio);
        descriptor.Field(x => x.LicenseNumber);
        descriptor.Field(x => x.CommissionPercentage);
        descriptor.Field(x => x.WalletId);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);

        descriptor
            .Field("person")
            .Type<ObjectType<Person>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                return await db.People
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == professional.PersonId, context.RequestAborted);
            });

        descriptor
            .Field("specialties")
            .Type<NonNullType<ListType<NonNullType<ObjectType<ProfessionalSpecialtyLink>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                return await db.ProfessionalSpecialtyLinks
                    .AsNoTracking()
                    .Where(l => l.ProfessionalId == professional.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor
            .Field("services")
            .Type<NonNullType<ListType<NonNullType<ObjectType<ProfessionalService>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                return await db.ProfessionalServices
                    .AsNoTracking()
                    .Where(ps => ps.ProfessionalId == professional.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor
            .Field("schedule")
            .Type<NonNullType<ListType<NonNullType<ObjectType<ProfessionalSchedule>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                return await db.ProfessionalSchedules
                    .AsNoTracking()
                    .Where(s => s.ProfessionalId == professional.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor
            .Field("wallet")
            .Type<ObjectType<Wallet>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                if (!professional.WalletId.HasValue) return null;
                return await db.Wallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == professional.WalletId.Value, context.RequestAborted);
            });

        descriptor
            .Field("appointments")
            .Type<NonNullType<ListType<NonNullType<ObjectType<Appointment>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var professional = context.Parent<Professional>();
                return await db.Appointments
                    .AsNoTracking()
                    .Where(a => a.ProfessionalId == professional.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Person);
        descriptor.Ignore(x => x.Wallet);
    }
}
