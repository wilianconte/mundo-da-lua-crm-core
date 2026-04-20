using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointmentTasks;

public record GetAppointmentTasksQuery(
    Guid? AppointmentId,
    int? Status
) : IRequest<Result<IReadOnlyList<AppointmentTaskDto>>>;
