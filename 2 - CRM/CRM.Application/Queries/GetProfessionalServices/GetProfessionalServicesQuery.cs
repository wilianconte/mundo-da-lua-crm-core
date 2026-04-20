using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalServices;

public record GetProfessionalServicesQuery(Guid ProfessionalId) : IRequest<Result<IReadOnlyList<ProfessionalServiceDto>>>;
