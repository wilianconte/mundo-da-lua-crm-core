using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalById;

public record GetProfessionalByIdQuery(Guid Id) : IRequest<Result<ProfessionalDto>>;
