using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalById;

public sealed class GetProfessionalByIdHandler : IRequestHandler<GetProfessionalByIdQuery, Result<ProfessionalDto>>
{
    private readonly IProfessionalRepository _repository;

    public GetProfessionalByIdHandler(IProfessionalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProfessionalDto>> Handle(GetProfessionalByIdQuery request, CancellationToken ct)
    {
        var professional = await _repository.GetByIdAsync(request.Id, ct);

        if (professional is null)
            return Result<ProfessionalDto>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");

        return Result<ProfessionalDto>.Success(professional.Adapt<ProfessionalDto>());
    }
}
