using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllStudentGuardians;

public sealed class GetAllStudentGuardiansHandler : IRequestHandler<GetAllStudentGuardiansQuery, Result<IReadOnlyList<StudentGuardianDto>>>
{
    private readonly IStudentGuardianRepository _repository;

    public GetAllStudentGuardiansHandler(IStudentGuardianRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<StudentGuardianDto>>> Handle(GetAllStudentGuardiansQuery request, CancellationToken ct)
    {
        var guardians = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<StudentGuardianDto>>.Success(guardians.Adapt<IReadOnlyList<StudentGuardianDto>>());
    }
}
