using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.CompleteAppointment;

public sealed record CompleteAppointmentCommand(
    Guid Id,
    Guid CategoryId
) : IRequest<Result<AppointmentDto>>;
