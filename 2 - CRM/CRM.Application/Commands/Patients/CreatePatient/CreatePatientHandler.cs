using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Patients.CreatePatient;

public sealed class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IPatientRepository _patientRepo;
    private readonly IPersonRepository  _personRepo;
    private readonly ITenantService     _tenant;

    public CreatePatientHandler(IPatientRepository patientRepo, IPersonRepository personRepo, ITenantService tenant)
    {
        _patientRepo = patientRepo;
        _personRepo  = personRepo;
        _tenant      = tenant;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(request.PersonId, ct);
        if (person is null)
            return Result<PatientDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        var duplicate = await _patientRepo.PersonAlreadyActivePatientAsync(
            _tenant.TenantId, request.PersonId, ct: ct);
        if (duplicate)
            return Result<PatientDto>.Failure(
                "PATIENT_DUPLICATE_PERSON",
                "This person already has a patient record in this tenant (RN-043).");

        var patient = Patient.Create(_tenant.TenantId, request.PersonId, request.Notes);
        await _patientRepo.AddAsync(patient, ct);
        await _patientRepo.SaveChangesAsync(ct);

        return Result<PatientDto>.Success(patient.Adapt<PatientDto>());
    }
}
