using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointments;

public record GetAppointmentsQuery(
    Guid? ProfessionalId,
    Guid? PatientId,
    int? Status,
    DateOnly? Date
) : IRequest<Result<IReadOnlyList<AppointmentDto>>>;
