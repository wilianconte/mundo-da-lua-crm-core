using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetPatientById;

public sealed class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IPatientRepository _repository;

    public GetPatientByIdHandler(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken ct)
    {
        var patient = await _repository.GetByIdAsync(request.Id, ct);

        if (patient is null)
            return Result<PatientDto>.Failure("PATIENT_NOT_FOUND", "Patient not found.");

        return Result<PatientDto>.Success(patient.Adapt<PatientDto>());
    }
}
