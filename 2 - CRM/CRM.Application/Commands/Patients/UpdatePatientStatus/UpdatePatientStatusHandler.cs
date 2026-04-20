using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Patients.UpdatePatientStatus;

public sealed class UpdatePatientStatusHandler : IRequestHandler<UpdatePatientStatusCommand, Result<PatientDto>>
{
    private readonly IPatientRepository _patientRepo;

    public UpdatePatientStatusHandler(IPatientRepository patientRepo) => _patientRepo = patientRepo;

    public async Task<Result<PatientDto>> Handle(UpdatePatientStatusCommand request, CancellationToken ct)
    {
        var patient = await _patientRepo.GetByIdAsync(request.Id, ct);
        if (patient is null)
            return Result<PatientDto>.Failure("PATIENT_NOT_FOUND", "Patient not found.");

        switch (request.TargetStatus)
        {
            case PatientStatus.Active:   patient.Activate();   break;
            case PatientStatus.Inactive: patient.Deactivate(); break;
            case PatientStatus.Blocked:  patient.Block();      break;
            default:
                return Result<PatientDto>.Failure("PATIENT_INVALID_STATUS", $"Invalid status {request.TargetStatus}.");
        }

        _patientRepo.Update(patient);
        await _patientRepo.SaveChangesAsync(ct);

        return Result<PatientDto>.Success(patient.Adapt<PatientDto>());
    }
}
