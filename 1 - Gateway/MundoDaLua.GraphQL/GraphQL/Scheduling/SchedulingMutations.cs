using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using MyCRM.CRM.Application.Commands.Professionals.CreateProfessional;
using MyCRM.CRM.Application.Commands.Professionals.UpdateProfessionalStatus;
using MyCRM.CRM.Application.Commands.Patients.CreatePatient;
using MyCRM.CRM.Application.Commands.Patients.UpdatePatientStatus;
using MyCRM.CRM.Application.Commands.Services.CreateService;
using MyCRM.CRM.Application.Commands.Services.UpdateService;
using MyCRM.CRM.Application.Commands.Services.DeactivateService;
using MyCRM.CRM.Application.Commands.ProfessionalServices.CreateProfessionalService;
using MyCRM.CRM.Application.Commands.CommissionRules.CreateCommissionRule;
using MyCRM.CRM.Application.Commands.ProfessionalSchedules.UpdateProfessionalSchedule;
using MyCRM.CRM.Application.Commands.Appointments.CreateAppointment;
using MyCRM.CRM.Application.Commands.Appointments.ConfirmAppointment;
using MyCRM.CRM.Application.Commands.Appointments.CancelAppointment;
using MyCRM.CRM.Application.Commands.Appointments.RescheduleAppointment;
using MyCRM.CRM.Application.Commands.Appointments.CompleteAppointment;
using MyCRM.CRM.Application.Commands.Appointments.MarkNoShow;
using MyCRM.CRM.Application.Commands.Appointments.ResolveAppointmentTask;
using MyCRM.CRM.Application.DTOs;
using MyCRM.GraphQL.GraphQL.Scheduling.Inputs;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Scheduling;

[MutationType]
public sealed class SchedulingMutations
{
    [Authorize(Policy = SystemPermissions.ProfessionalsCreate)]
    public async Task<ProfessionalPayload> CreateProfessionalAsync(
        CreateProfessionalInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateProfessionalCommand(
                input.PersonId,
                input.SpecialtyIds,
                input.Bio,
                input.LicenseNumber,
                input.CommissionPercentage), ct);

        return result.IsSuccess
            ? new ProfessionalPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ProfessionalsUpdate)]
    public async Task<ProfessionalPayload> UpdateProfessionalStatusAsync(
        UpdateProfessionalStatusInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateProfessionalStatusCommand(input.Id, input.TargetStatus), ct);

        return result.IsSuccess
            ? new ProfessionalPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.PatientsCreate)]
    public async Task<PatientPayload> CreatePatientAsync(
        CreatePatientInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePatientCommand(input.PersonId, input.Notes), ct);

        return result.IsSuccess
            ? new PatientPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.PatientsUpdate)]
    public async Task<PatientPayload> UpdatePatientStatusAsync(
        UpdatePatientStatusInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePatientStatusCommand(input.Id, input.TargetStatus), ct);

        return result.IsSuccess
            ? new PatientPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ServicesCreate)]
    public async Task<ServicePayload> CreateServiceAsync(
        CreateServiceInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateServiceCommand(
                input.Name,
                input.DefaultPrice,
                input.DefaultDurationInMinutes,
                input.Description), ct);

        return result.IsSuccess
            ? new ServicePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ServicesUpdate)]
    public async Task<ServicePayload> UpdateServiceAsync(
        UpdateServiceInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateServiceCommand(
                input.Id,
                input.Name,
                input.DefaultPrice,
                input.DefaultDurationInMinutes,
                input.Description), ct);

        return result.IsSuccess
            ? new ServicePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ServicesUpdate)]
    public async Task<ServicePayload> DeactivateServiceAsync(
        DeactivateServiceInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeactivateServiceCommand(input.Id), ct);

        return result.IsSuccess
            ? new ServicePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ProfessionalServicesCreate)]
    public async Task<ProfessionalServicePayload> CreateProfessionalServiceAsync(
        CreateProfessionalServiceInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateProfessionalServiceCommand(
                input.ProfessionalId,
                input.ServiceId,
                input.CustomPrice,
                input.CustomDurationInMinutes), ct);

        return result.IsSuccess
            ? new ProfessionalServicePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.CommissionRulesCreate)]
    public async Task<CommissionRulePayload> CreateCommissionRuleAsync(
        CreateCommissionRuleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateCommissionRuleCommand(
                input.CompanyPercentage,
                input.ProfessionalId,
                input.ServiceId), ct);

        return result.IsSuccess
            ? new CommissionRulePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.ProfessionalSchedulesUpdate)]
    public async Task<ProfessionalSchedulePayload> UpdateProfessionalScheduleAsync(
        UpdateProfessionalScheduleInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateProfessionalScheduleCommand(
                input.Id,
                input.StartTime,
                input.EndTime,
                input.IsAvailable), ct);

        return result.IsSuccess
            ? new ProfessionalSchedulePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsCreate)]
    public async Task<CreateAppointmentPayload> CreateAppointmentAsync(
        CreateAppointmentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var command = new CreateAppointmentCommand(
            input.ProfessionalId,
            input.PatientId,
            input.ServiceId,
            input.StartDateTime,
            input.Type,
            input.OverridePrice,
            input.PaymentReceiver,
            input.PaymentMethodId,
            input.Address is null ? null : new MyCRM.CRM.Application.Commands.Appointments.CreateAppointment.AddressInput(
                input.Address.Street,
                input.Address.Number,
                input.Address.Complement,
                input.Address.Neighborhood,
                input.Address.City,
                input.Address.State,
                input.Address.ZipCode,
                input.Address.Country),
            input.MeetingLink,
            input.Notes,
            input.Recurrence is null ? null : new MyCRM.CRM.Application.Commands.Appointments.CreateAppointment.RecurrenceInput(
                input.Recurrence.Frequency,
                input.Recurrence.EndDate,
                input.Recurrence.MaxOccurrences));

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? new CreateAppointmentPayload(result.Value!.Appointment, result.Value!.RecurringAppointments, result.Value!.Warnings)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsUpdate)]
    public async Task<AppointmentPayload> ConfirmAppointmentAsync(
        ConfirmAppointmentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ConfirmAppointmentCommand(input.Id, input.ConfirmedBy), ct);

        return result.IsSuccess
            ? new AppointmentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsUpdate)]
    public async Task<AppointmentPayload> CancelAppointmentAsync(
        CancelAppointmentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CancelAppointmentCommand(input.Id, input.Reason), ct);

        return result.IsSuccess
            ? new AppointmentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsUpdate)]
    public async Task<AppointmentPayload> RescheduleAppointmentAsync(
        RescheduleAppointmentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RescheduleAppointmentCommand(
                input.Id,
                input.NewStartDateTime,
                input.OverridePrice), ct);

        return result.IsSuccess
            ? new AppointmentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsUpdate)]
    public async Task<AppointmentPayload> CompleteAppointmentAsync(
        CompleteAppointmentInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CompleteAppointmentCommand(input.Id, input.CategoryId), ct);

        return result.IsSuccess
            ? new AppointmentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentsUpdate)]
    public async Task<AppointmentPayload> MarkNoShowAsync(
        MarkNoShowInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new MarkNoShowCommand(input.Id), ct);

        return result.IsSuccess
            ? new AppointmentPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }

    [Authorize(Policy = SystemPermissions.AppointmentTasksManage)]
    public async Task<AppointmentTaskPayload> ResolveAppointmentTaskAsync(
        ResolveAppointmentTaskInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ResolveAppointmentTaskCommand(
                input.TaskId,
                input.ApplyPenalty,
                input.PenaltyAmount,
                input.CategoryId,
                input.PaymentMethodId), ct);

        return result.IsSuccess
            ? new AppointmentTaskPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e => ErrorBuilder.New()
                    .SetMessage(e)
                    .SetExtension("code", result.ErrorCode)
                    .Build()));
    }
}
