using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentHandler : IRequestHandler<RescheduleAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IProfessionalServiceRepository _professionalServiceRepo;
    private readonly IProfessionalScheduleRepository _scheduleRepo;

    public RescheduleAppointmentHandler(
        IAppointmentRepository appointmentRepo,
        IServiceRepository serviceRepo,
        IProfessionalServiceRepository professionalServiceRepo,
        IProfessionalScheduleRepository scheduleRepo)
    {
        _appointmentRepo = appointmentRepo;
        _serviceRepo = serviceRepo;
        _professionalServiceRepo = professionalServiceRepo;
        _scheduleRepo = scheduleRepo;
    }

    public async Task<Result<AppointmentDto>> Handle(RescheduleAppointmentCommand request, CancellationToken ct)
    {
        var warnings = new List<string>();

        var appointment = await _appointmentRepo.GetByIdAsync(request.Id, ct);
        if (appointment is null)
            return Result<AppointmentDto>.Failure("APPOINTMENT_NOT_FOUND", "Appointment not found.");

        if (appointment.Status != AppointmentStatus.Suggested && appointment.Status != AppointmentStatus.Confirmed)
            return Result<AppointmentDto>.Failure("APPOINTMENT_INVALID_STATUS", $"Cannot reschedule appointment with status {appointment.Status}.");

        var service = await _serviceRepo.GetByIdAsync(appointment.ServiceId, ct);
        if (service is null)
            return Result<AppointmentDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

        var allPS = await _professionalServiceRepo.GetAllAsync(ct);
        var professionalService = allPS.FirstOrDefault(ps =>
            ps.ProfessionalId == appointment.ProfessionalId && ps.ServiceId == appointment.ServiceId && ps.IsActive);

        var durationMinutes = professionalService?.CustomDurationInMinutes ?? service.DefaultDurationInMinutes;
        var newEndDateTime = request.NewStartDateTime.AddMinutes(durationMinutes);

        var hasConflict = await _appointmentRepo.HasConflictAsync(
            appointment.ProfessionalId, request.NewStartDateTime, newEndDateTime, excludeId: appointment.Id, ct: ct);
        if (hasConflict)
            warnings.Add("Time conflict detected for this professional at the new time (RN-054).");

        var schedules = await _scheduleRepo.GetByProfessionalAsync(appointment.ProfessionalId, ct);
        var dayOfWeek = (int)request.NewStartDateTime.DayOfWeek;
        var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
        if (daySchedule is null)
            warnings.Add("No schedule configured for this professional on the new day of week (RN-059).");
        else if (!daySchedule.IsAvailable)
            warnings.Add("Professional is unavailable on the new day of week (RN-059).");
        else
        {
            var startTime = request.NewStartDateTime.TimeOfDay;
            if (startTime < daySchedule.StartTime || startTime.Add(TimeSpan.FromMinutes(durationMinutes)) > daySchedule.EndTime)
                warnings.Add("New appointment time is outside the professional's working hours (RN-059).");
        }

        var previousId = appointment.Id;
        appointment.MarkRescheduled();

        var rescheduled = Appointment.Create(
            appointment.TenantId,
            appointment.ProfessionalId,
            appointment.PatientId,
            appointment.ServiceId,
            request.NewStartDateTime,
            newEndDateTime,
            appointment.Type,
            request.OverridePrice ?? appointment.Price,
            appointment.PaymentReceiver,
            appointment.PaymentMethodId,
            appointment.Address,
            appointment.MeetingLink,
            recurrenceId: appointment.RecurrenceId,
            rescheduledFrom: previousId,
            notes: appointment.Notes);

        _appointmentRepo.Update(appointment);
        await _appointmentRepo.AddAsync(rescheduled, ct);
        await _appointmentRepo.SaveChangesAsync(ct);

        var dto = rescheduled.Adapt<AppointmentDto>();
        return Result<AppointmentDto>.Success(dto with { Warnings = warnings });
    }
}
