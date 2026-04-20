using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.ProfessionalServices.CreateProfessionalService;

public sealed record CreateProfessionalServiceCommand(
    Guid ProfessionalId,
    Guid ServiceId,
    decimal? CustomPrice,
    int? CustomDurationInMinutes
) : IRequest<Result<ProfessionalServiceDto>>;
