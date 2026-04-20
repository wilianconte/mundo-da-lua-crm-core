using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.ConfirmAppointment;

public sealed class ConfirmAppointmentHandler : IRequestHandler<ConfirmAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;

    public ConfirmAppointmentHandler(IAppointmentRepository appointmentRepo)
    {
        _appointmentRepo = appointmentRepo;
    }

    public async Task<Result<AppointmentDto>> Handle(ConfirmAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        if (appointment.Status == AppointmentStatus.Confirmed)
        {
            var dto = appointment.Adapt<AppointmentDto>();
            return Result<AppointmentDto>.Success(dto with { Warnings = new List<string> { "Appointment already confirmed (RN-051)." } });
        }

        if (appointment.Status != AppointmentStatus.Suggested)
            return Result<AppointmentDto>.Failure("APPOINTMENT_INVALID_STATUS", $"Cannot confirm appointment with status {appointment.Status}.");

        appointment.Confirm(request.ConfirmedBy);
        _appointmentRepo.Update(appointment);
        await _appointmentRepo.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.Adapt<AppointmentDto>() with { Warnings = new List<string>() });
    }
}
