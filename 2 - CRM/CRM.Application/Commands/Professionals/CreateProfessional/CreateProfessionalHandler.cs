using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Professionals.CreateProfessional;

public sealed class CreateProfessionalHandler : IRequestHandler<CreateProfessionalCommand, Result<ProfessionalDto>>
{
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IProfessionalSpecialtyRepository _specialtyRepo;
    private readonly IProfessionalSpecialtyLinkRepository _linkRepo;
    private readonly IPersonRepository _personRepo;
    private readonly ITenantService _tenant;

    public CreateProfessionalHandler(
        IProfessionalRepository professionalRepo,
        IProfessionalSpecialtyRepository specialtyRepo,
        IProfessionalSpecialtyLinkRepository linkRepo,
        IPersonRepository personRepo,
        ITenantService tenant)
    {
        _professionalRepo = professionalRepo;
        _specialtyRepo    = specialtyRepo;
        _linkRepo         = linkRepo;
        _personRepo       = personRepo;
        _tenant           = tenant;
    }

    public async Task<Result<ProfessionalDto>> Handle(CreateProfessionalCommand request, CancellationToken ct)
    {
        var person = await _personRepo.GetByIdAsync(request.PersonId, ct);
        if (person is null)
            return Result<ProfessionalDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        var duplicate = await _professionalRepo.PersonAlreadyActiveProfessionalAsync(
            _tenant.TenantId, request.PersonId, ct: ct);
        if (duplicate)
            return Result<ProfessionalDto>.Failure(
                "PROFESSIONAL_DUPLICATE_PERSON",
                "This person already has a professional record in this tenant (RN-042).");

        // Validate all specialty IDs exist
        foreach (var specialtyId in request.SpecialtyIds)
        {
            var specialty = await _specialtyRepo.GetByIdAsync(specialtyId, ct);
            if (specialty is null)
                return Result<ProfessionalDto>.Failure(
                    "SPECIALTY_NOT_FOUND", $"Specialty {specialtyId} not found.");
        }

        var professional = Professional.Create(
            _tenant.TenantId,
            request.PersonId,
            request.Bio,
            request.LicenseNumber,
            request.CommissionPercentage);

        await _professionalRepo.AddAsync(professional, ct);
        await _professionalRepo.SaveChangesAsync(ct);

        foreach (var specialtyId in request.SpecialtyIds.Distinct())
        {
            var link = ProfessionalSpecialtyLink.Create(_tenant.TenantId, professional.Id, specialtyId);
            await _linkRepo.AddAsync(link, ct);
        }
        await _linkRepo.SaveChangesAsync(ct);

        return Result<ProfessionalDto>.Success(professional.Adapt<ProfessionalDto>());
    }
}
