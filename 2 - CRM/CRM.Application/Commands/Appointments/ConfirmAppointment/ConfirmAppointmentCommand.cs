using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.ConfirmAppointment;

public sealed record ConfirmAppointmentCommand(
    Guid Id,
    string ConfirmedBy
) : IRequest<Result<AppointmentDto>>;
