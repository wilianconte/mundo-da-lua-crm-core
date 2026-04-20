using MyCRM.CRM.Application.DTOs;

namespace MyCRM.CRM.Application.DTOs;

public sealed record CreateAppointmentResult(
    AppointmentDto Appointment,
    IReadOnlyList<AppointmentDto> RecurringAppointments,
    List<string> Warnings
);
