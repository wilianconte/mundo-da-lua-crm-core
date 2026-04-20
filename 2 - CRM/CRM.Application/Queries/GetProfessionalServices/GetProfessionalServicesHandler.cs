using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalServices;

public sealed class GetProfessionalServicesHandler : IRequestHandler<GetProfessionalServicesQuery, Result<IReadOnlyList<ProfessionalServiceDto>>>
{
    private readonly IProfessionalServiceRepository _repository;
    private readonly IProfessionalRepository _professionalRepo;

    public GetProfessionalServicesHandler(
        IProfessionalServiceRepository repository,
        IProfessionalRepository professionalRepo)
    {
        _repository = repository;
        _professionalRepo = professionalRepo;
    }

    public async Task<Result<IReadOnlyList<ProfessionalServiceDto>>> Handle(GetProfessionalServicesQuery request, CancellationToken ct)
    {
        var professional = await _professionalRepo.GetByIdAsync(request.ProfessionalId, ct);
        if (professional is null)
            return Result<IReadOnlyList<ProfessionalServiceDto>>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");

        var all = await _repository.GetAllAsync(ct);
        var filtered = all.Where(ps => ps.ProfessionalId == request.ProfessionalId).ToList();
        var dtos = filtered.Adapt<IReadOnlyList<ProfessionalServiceDto>>();
        return Result<IReadOnlyList<ProfessionalServiceDto>>.Success(dtos);
    }
}
