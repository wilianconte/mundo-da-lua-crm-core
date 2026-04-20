using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetServiceById;

public sealed class GetServiceByIdHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
{
    private readonly IServiceRepository _repository;

    public GetServiceByIdHandler(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken ct)
    {
        var service = await _repository.GetByIdAsync(request.Id, ct);

        if (service is null)
            return Result<ServiceDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

        return Result<ServiceDto>.Success(service.Adapt<ServiceDto>());
    }
}
