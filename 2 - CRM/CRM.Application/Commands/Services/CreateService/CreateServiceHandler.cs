using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Services.CreateService;

public sealed class CreateServiceHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepo;
    private readonly ITenantService _tenant;

    public CreateServiceHandler(IServiceRepository serviceRepo, ITenantService tenant)
    {
        _serviceRepo = serviceRepo;
        _tenant = tenant;
    }

    public async Task<Result<ServiceDto>> Handle(CreateServiceCommand request, CancellationToken ct)
    {
        var nameExists = await _serviceRepo.NameExistsAsync(_tenant.TenantId, request.Name, ct: ct);
        if (nameExists)
            return Result<ServiceDto>.Failure("SERVICE_NAME_DUPLICATE", "A service with this name already exists.");

        var service = Service.Create(
            _tenant.TenantId,
            request.Name,
            request.DefaultPrice,
            request.DefaultDurationInMinutes,
            request.Description);

        await _serviceRepo.AddAsync(service, ct);
        await _serviceRepo.SaveChangesAsync(ct);

        return Result<ServiceDto>.Success(service.Adapt<ServiceDto>());
    }
}
