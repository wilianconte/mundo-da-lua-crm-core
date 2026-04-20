using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid Id,
    DateTime NewStartDateTime,
    decimal? OverridePrice
) : IRequest<Result<AppointmentDto>>;
