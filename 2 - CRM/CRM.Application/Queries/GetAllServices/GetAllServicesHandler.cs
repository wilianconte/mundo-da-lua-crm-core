using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllServices;

public sealed class GetAllServicesHandler : IRequestHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceDto>>>
{
    private readonly IServiceRepository _repository;

    public GetAllServicesHandler(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ServiceDto>>> Handle(GetAllServicesQuery request, CancellationToken ct)
    {
        var services = await _repository.GetAllAsync(ct);
        var dtos = services.Adapt<IReadOnlyList<ServiceDto>>();
        return Result<IReadOnlyList<ServiceDto>>.Success(dtos);
    }
}
