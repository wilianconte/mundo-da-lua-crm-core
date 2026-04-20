using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetProfessionalSchedules;

public sealed class GetProfessionalSchedulesHandler : IRequestHandler<GetProfessionalSchedulesQuery, Result<IReadOnlyList<ProfessionalScheduleDto>>>
{
    private readonly IProfessionalScheduleRepository _repository;

    public GetProfessionalSchedulesHandler(IProfessionalScheduleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProfessionalScheduleDto>>> Handle(GetProfessionalSchedulesQuery request, CancellationToken ct)
    {
        var schedules = await _repository.GetByProfessionalAsync(request.ProfessionalId, ct);
        var dtos = schedules.Adapt<IReadOnlyList<ProfessionalScheduleDto>>();
        return Result<IReadOnlyList<ProfessionalScheduleDto>>.Success(dtos);
    }
}
