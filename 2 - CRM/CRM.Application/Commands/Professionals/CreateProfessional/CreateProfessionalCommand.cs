using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Professionals.CreateProfessional;

public sealed record CreateProfessionalCommand(
    Guid PersonId,
    List<Guid> SpecialtyIds,
    string? Bio,
    string? LicenseNumber,
    decimal? CommissionPercentage
) : IRequest<Result<ProfessionalDto>>;
