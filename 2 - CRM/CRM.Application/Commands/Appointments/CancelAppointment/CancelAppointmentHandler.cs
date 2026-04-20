using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.CancelAppointment;

public sealed class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public CancelAppointmentHandler(IAppointmentRepository appointmentRepo)
    {
        _appointmentRepo = appointmentRepo;
    }

    public async Task<Result<AppointmentDto>> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Suggested && appointment.Status != AppointmentStatus.Confirmed)
            return Result<AppointmentDto>.Failure("APPOINTMENT_INVALID_STATUS", $"Cannot cancel appointment with status {appointment.Status}.");

        var hoursUntilAppointment = (appointment.StartDateTime - DateTime.UtcNow).TotalHours;
        var lateNotice = hoursUntilAppointment < request.MinimumNoticeHours;

        appointment.Cancel(request.Reason, lateNotice);
        _appointmentRepo.Update(appointment);
        await _appointmentRepo.SaveChangesAsync(ct);

        var warnings = new List<string>();
        if (lateNotice)
            warnings.Add($"Cancellation with less than {request.MinimumNoticeHours}h notice (RN-052).");

        var dto = appointment.Adapt<AppointmentDto>();
        return Result<AppointmentDto>.Success(dto with { Warnings = warnings });
    }
}
