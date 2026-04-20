using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.MarkNoShow;

public sealed record MarkNoShowCommand(
    Guid Id
) : IRequest<Result<AppointmentDto>>;
