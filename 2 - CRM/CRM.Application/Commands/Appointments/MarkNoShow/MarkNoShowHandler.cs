using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.MarkNoShow;

public sealed class MarkNoShowHandler : IRequestHandler<MarkNoShowCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IAppointmentTaskRepository _taskRepo;
    private readonly ITenantService _tenant;

    public MarkNoShowHandler(
        IAppointmentRepository appointmentRepo,
        IAppointmentTaskRepository taskRepo,
        ITenantService tenant)
    {
        _appointmentRepo = appointmentRepo;
        _taskRepo = taskRepo;
        _tenant = tenant;
    }

    public async Task<Result<AppointmentDto>> Handle(MarkNoShowCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            return Result<AppointmentDto>.Failure("APPOINTMENT_INVALID_STATUS", "Only Confirmed appointments can be marked as NoShow.");

        appointment.MarkNoShow();
        _appointmentRepo.Update(appointment);
        await _appointmentRepo.SaveChangesAsync(ct);

        var task = AppointmentTask.Create(
            _tenant.TenantId,
            appointment.Id,
            AppointmentTaskType.NoShowDecision,
            assignedToRole: "Professional");

        await _taskRepo.AddAsync(task, ct);
        await _taskRepo.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.Adapt<AppointmentDto>() with { Warnings = new List<string>() });
    }
}
