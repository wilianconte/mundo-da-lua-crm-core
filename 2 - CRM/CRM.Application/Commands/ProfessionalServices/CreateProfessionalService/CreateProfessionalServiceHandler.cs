using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.ProfessionalServices.CreateProfessionalService;

public sealed class CreateProfessionalServiceHandler : IRequestHandler<CreateProfessionalServiceCommand, Result<ProfessionalServiceDto>>
{
    private readonly IProfessionalServiceRepository _professionalServiceRepo;
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ITenantService _tenant;

    public CreateProfessionalServiceHandler(
        IProfessionalServiceRepository professionalServiceRepo,
        IProfessionalRepository professionalRepo,
        IServiceRepository serviceRepo,
        ITenantService tenant)
    {
        _professionalServiceRepo = professionalServiceRepo;
        _professionalRepo = professionalRepo;
        _serviceRepo = serviceRepo;
        _tenant = tenant;
    }

    public async Task<Result<ProfessionalServiceDto>> Handle(CreateProfessionalServiceCommand request, CancellationToken ct)
    {
        var professional = await _professionalRepo.GetByIdAsync(request.ProfessionalId, ct);
        if (professional is null)
            return Result<ProfessionalServiceDto>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");

        if (professional.Status != ProfessionalStatus.Active)
            return Result<ProfessionalServiceDto>.Failure("PROFESSIONAL_NOT_ACTIVE", "Professional must be Active to link services.");

        var service = await _serviceRepo.GetByIdAsync(request.ServiceId, ct);
        if (service is null)
            return Result<ProfessionalServiceDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

        if (!service.IsActive)
            return Result<ProfessionalServiceDto>.Failure("SERVICE_NOT_ACTIVE", "Service must be active to link to a professional (RN-067).");

        var linkExists = await _professionalServiceRepo.LinkExistsAsync(request.ProfessionalId, request.ServiceId, ct: ct);
        if (linkExists)
            return Result<ProfessionalServiceDto>.Failure("PROFESSIONAL_SERVICE_DUPLICATE", "This service is already linked to this professional.");

        var professionalService = ProfessionalService.Create(
            _tenant.TenantId,
            request.ProfessionalId,
            request.ServiceId,
            request.CustomPrice,
            request.CustomDurationInMinutes);

        await _professionalServiceRepo.AddAsync(professionalService, ct);
        await _professionalServiceRepo.SaveChangesAsync(ct);

        return Result<ProfessionalServiceDto>.Success(professionalService.Adapt<ProfessionalServiceDto>());
    }
}
