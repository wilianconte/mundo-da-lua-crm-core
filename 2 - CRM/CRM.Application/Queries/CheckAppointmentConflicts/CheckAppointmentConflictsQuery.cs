using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.CheckAppointmentConflicts;

public record CheckAppointmentConflictsQuery(
    Guid ProfessionalId,
    DateTime StartDateTime,
    DateTime EndDateTime,
    Guid? ExcludeAppointmentId
) : IRequest<Result<IReadOnlyList<AppointmentDto>>>;
