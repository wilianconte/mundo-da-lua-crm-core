using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDto>>;
