using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalSchedules;

public record GetProfessionalSchedulesQuery(Guid ProfessionalId) : IRequest<Result<IReadOnlyList<ProfessionalScheduleDto>>>;
