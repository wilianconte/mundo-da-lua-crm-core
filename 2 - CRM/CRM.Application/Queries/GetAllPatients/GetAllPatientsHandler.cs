using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllPatients;

public sealed class GetAllPatientsHandler : IRequestHandler<GetAllPatientsQuery, Result<IReadOnlyList<PatientDto>>>
{
    private readonly IPatientRepository _repository;

    public GetAllPatientsHandler(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<PatientDto>>> Handle(GetAllPatientsQuery request, CancellationToken ct)
    {
        var patients = await _repository.GetAllAsync(ct);
        var dtos = patients.Adapt<IReadOnlyList<PatientDto>>();
        return Result<IReadOnlyList<PatientDto>>.Success(dtos);
    }
}
