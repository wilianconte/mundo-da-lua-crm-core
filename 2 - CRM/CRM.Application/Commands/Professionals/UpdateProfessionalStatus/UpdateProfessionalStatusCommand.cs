using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Professionals.UpdateProfessionalStatus;

public sealed record UpdateProfessionalStatusCommand(
    Guid Id,
    ProfessionalStatus TargetStatus
) : IRequest<Result<ProfessionalDto>>;
