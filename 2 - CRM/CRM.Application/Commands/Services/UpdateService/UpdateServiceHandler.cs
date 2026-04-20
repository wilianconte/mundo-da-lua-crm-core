using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Services.UpdateService;

public sealed class UpdateServiceHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepo;
    private readonly ITenantService _tenant;

    public UpdateServiceHandler(IServiceRepository serviceRepo, ITenantService tenant)
    {
        _serviceRepo = serviceRepo;
        _tenant = tenant;
    }

    public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken ct)
    {
        var service = await _serviceRepo.GetByIdAsync(request.Id, ct);
        if (service is null)
            return Result<ServiceDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

        var nameExists = await _serviceRepo.NameExistsAsync(_tenant.TenantId, request.Name, excludeId: request.Id, ct: ct);
        if (nameExists)
            return Result<ServiceDto>.Failure("SERVICE_NAME_DUPLICATE", "A service with this name already exists.");

        service.Update(request.Name, request.DefaultPrice, request.DefaultDurationInMinutes, request.Description);
        _serviceRepo.Update(service);
        await _serviceRepo.SaveChangesAsync(ct);

        return Result<ServiceDto>.Success(service.Adapt<ServiceDto>());
    }
}
