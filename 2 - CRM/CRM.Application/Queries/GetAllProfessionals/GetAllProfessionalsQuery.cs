using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllProfessionals;

public record GetAllProfessionalsQuery : IRequest<Result<IReadOnlyList<ProfessionalDto>>>;
