using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.GraphQL.Scheduling.Types;

public sealed class AppointmentObjectType : ObjectType<Appointment>
{
    protected override void Configure(IObjectTypeDescriptor<Appointment> descriptor)
    {
        descriptor.Name("Appointment");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.ProfessionalId);
        descriptor.Field(x => x.PatientId);
        descriptor.Field(x => x.ServiceId);
        descriptor.Field(x => x.StartDateTime);
        descriptor.Field(x => x.EndDateTime);
        descriptor.Field(x => x.Type);
        descriptor.Field(x => x.Price);
        descriptor.Field(x => x.Address);
        descriptor.Field(x => x.MeetingLink);
        descriptor.Field(x => x.PaymentReceiver);
        descriptor.Field(x => x.PaymentMethodId);
        descriptor.Field(x => x.Status);
        descriptor.Field(x => x.RecurrenceId);
        descriptor.Field(x => x.ConfirmedBy);
        descriptor.Field(x => x.ConfirmedAt);
        descriptor.Field(x => x.CancellationReason);
        descriptor.Field(x => x.CancelledAt);
        descriptor.Field(x => x.CancelledWithLateNotice);
        descriptor.Field(x => x.NoShowAt);
        descriptor.Field(x => x.CompletedAt);
        descriptor.Field(x => x.RescheduledFrom);
        descriptor.Field(x => x.RescheduledAt);
        descriptor.Field(x => x.TransactionId);
        descriptor.Field(x => x.Notes);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);

        descriptor
            .Field("professional")
            .Type<ObjectType<Professional>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                return await db.Professionals
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == appointment.ProfessionalId, context.RequestAborted);
            });

        descriptor
            .Field("patient")
            .Type<ObjectType<Patient>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                return await db.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == appointment.PatientId, context.RequestAborted);
            });

        descriptor
            .Field("service")
            .Type<ObjectType<Service>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                return await db.Services
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId, context.RequestAborted);
            });

        descriptor
            .Field("recurrence")
            .Type<ObjectType<AppointmentRecurrence>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                if (!appointment.RecurrenceId.HasValue) return null;
                return await db.AppointmentRecurrences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == appointment.RecurrenceId.Value, context.RequestAborted);
            });

        descriptor
            .Field("tasks")
            .Type<NonNullType<ListType<NonNullType<ObjectType<AppointmentTask>>>>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                return await db.AppointmentTasks
                    .AsNoTracking()
                    .Where(t => t.AppointmentId == appointment.Id)
                    .ToListAsync(context.RequestAborted);
            });

        descriptor
            .Field("transaction")
            .Type<ObjectType<Transaction>>()
            .Resolve(async context =>
            {
                var db = context.Service<CRMDbContext>();
                var appointment = context.Parent<Appointment>();
                if (!appointment.TransactionId.HasValue) return null;
                return await db.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == appointment.TransactionId.Value, context.RequestAborted);
            });

        descriptor.Ignore(x => x.TenantId);
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
        descriptor.Ignore(x => x.Professional);
        descriptor.Ignore(x => x.Patient);
        descriptor.Ignore(x => x.Service);
        descriptor.Ignore(x => x.PaymentMethod);
        descriptor.Ignore(x => x.Recurrence);
    }
}
