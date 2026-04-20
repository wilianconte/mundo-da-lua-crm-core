using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.CancelAppointment;

public sealed record CancelAppointmentCommand(
    Guid Id,
    string? Reason,
    int MinimumNoticeHours = 24
) : IRequest<Result<AppointmentDto>>;
