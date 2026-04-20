using HotChocolate.Authorization;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Application.Queries.GetAllProfessionals;
using MyCRM.CRM.Application.Queries.GetProfessionalById;
using MyCRM.CRM.Application.Queries.GetAllPatients;
using MyCRM.CRM.Application.Queries.GetPatientById;
using MyCRM.CRM.Application.Queries.GetAllServices;
using MyCRM.CRM.Application.Queries.GetServiceById;
using MyCRM.CRM.Application.Queries.GetProfessionalServices;
using MyCRM.CRM.Application.Queries.GetAllCommissionRules;
using MyCRM.CRM.Application.Queries.GetProfessionalSchedules;
using MyCRM.CRM.Application.Queries.GetAppointments;
using MyCRM.CRM.Application.Queries.GetAppointmentById;
using MyCRM.CRM.Application.Queries.GetAppointmentTasks;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Scheduling;

[QueryType]
public sealed class SchedulingQueries
{
    [Authorize(Policy = SystemPermissions.ProfessionalsRead)]
    public async Task<IReadOnlyList<ProfessionalDto>> ProfessionalsAsync(
        ProfessionalStatus? status,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAllProfessionalsQuery(), ct);
        if (!result.IsSuccess) return [];
        var list = result.Value!;
        if (status.HasValue)
            list = list.Where(p => p.Status == status.Value).ToList();
        return list;
    }

    [Authorize(Policy = SystemPermissions.ProfessionalsRead)]
    public async Task<ProfessionalDto?> ProfessionalAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetProfessionalByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = SystemPermissions.PatientsRead)]
    public async Task<IReadOnlyList<PatientDto>> PatientsAsync(
        PatientStatus? status,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAllPatientsQuery(), ct);
        if (!result.IsSuccess) return [];
        var list = result.Value!;
        if (status.HasValue)
            list = list.Where(p => p.Status == status.Value).ToList();
        return list;
    }

    [Authorize(Policy = SystemPermissions.PatientsRead)]
    public async Task<PatientDto?> PatientAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPatientByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = SystemPermissions.ServicesRead)]
    public async Task<IReadOnlyList<ServiceDto>> ServicesAsync(
        bool? isActive,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAllServicesQuery(), ct);
        if (!result.IsSuccess) return [];
        var list = result.Value!;
        if (isActive.HasValue)
            list = list.Where(s => s.IsActive == isActive.Value).ToList();
        return list;
    }

    [Authorize(Policy = SystemPermissions.ServicesRead)]
    public async Task<ServiceDto?> ServiceAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetServiceByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = SystemPermissions.AppointmentsRead)]
    public async Task<IReadOnlyList<AppointmentDto>> AppointmentsAsync(
        Guid? professionalId,
        Guid? patientId,
        int? status,
        DateOnly? date,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetAppointmentsQuery(professionalId, patientId, status, date), ct);
        return result.IsSuccess ? result.Value! : [];
    }

    [Authorize(Policy = SystemPermissions.AppointmentsRead)]
    public async Task<AppointmentDto?> AppointmentAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAppointmentByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = SystemPermissions.AppointmentTasksRead)]
    public async Task<IReadOnlyList<AppointmentTaskDto>> AppointmentTasksAsync(
        Guid? appointmentId,
        int? status,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetAppointmentTasksQuery(appointmentId, status), ct);
        return result.IsSuccess ? result.Value! : [];
    }
}
