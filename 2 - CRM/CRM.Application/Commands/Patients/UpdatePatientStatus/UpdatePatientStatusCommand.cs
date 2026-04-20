using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Patients.UpdatePatientStatus;

public sealed record UpdatePatientStatusCommand(
    Guid Id,
    PatientStatus TargetStatus
) : IRequest<Result<PatientDto>>;
