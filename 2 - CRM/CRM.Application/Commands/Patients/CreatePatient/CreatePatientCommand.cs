using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;
using MediatR;

namespace MyCRM.CRM.Application.Commands.Patients.CreatePatient;

public sealed record CreatePatientCommand(
    Guid PersonId,
    string? Notes
) : IRequest<Result<PatientDto>>;
