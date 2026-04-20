using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Appointments.ResolveAppointmentTask;

public sealed record ResolveAppointmentTaskCommand(
    Guid TaskId,
    bool ApplyPenalty,
    decimal PenaltyAmount,
    Guid CategoryId,
    Guid PaymentMethodId
) : IRequest<Result<AppointmentTaskDto>>;
