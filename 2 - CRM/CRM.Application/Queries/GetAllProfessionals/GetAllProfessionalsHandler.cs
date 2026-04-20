using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllProfessionals;

public sealed class GetAllProfessionalsHandler : IRequestHandler<GetAllProfessionalsQuery, Result<IReadOnlyList<ProfessionalDto>>>
{
    private readonly IProfessionalRepository _repository;

    public GetAllProfessionalsHandler(IProfessionalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProfessionalDto>>> Handle(GetAllProfessionalsQuery request, CancellationToken ct)
    {
        var professionals = await _repository.GetAllAsync(ct);
        var dtos = professionals.Adapt<IReadOnlyList<ProfessionalDto>>();
        return Result<IReadOnlyList<ProfessionalDto>>.Success(dtos);
    }
}
