using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Services.DeactivateService;

public sealed class DeactivateServiceHandler : IRequestHandler<DeactivateServiceCommand, Result<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepo;

    public DeactivateServiceHandler(IServiceRepository serviceRepo) => _serviceRepo = serviceRepo;

    public async Task<Result<ServiceDto>> Handle(DeactivateServiceCommand request, CancellationToken ct)
    {
        var service = await _serviceRepo.GetByIdAsync(request.Id, ct);
        if (service is null)
            return Result<ServiceDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

        service.Deactivate();
        _serviceRepo.Update(service);
        await _serviceRepo.SaveChangesAsync(ct);

        return Result<ServiceDto>.Success(service.Adapt<ServiceDto>());
    }
}
