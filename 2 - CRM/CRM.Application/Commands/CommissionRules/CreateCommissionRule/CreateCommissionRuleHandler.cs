using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.CommissionRules.CreateCommissionRule;

public sealed class CreateCommissionRuleHandler : IRequestHandler<CreateCommissionRuleCommand, Result<CommissionRuleDto>>
{
    private readonly ICommissionRuleRepository _commissionRuleRepo;
    private readonly IProfessionalRepository _professionalRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ITenantService _tenant;

    public CreateCommissionRuleHandler(
        ICommissionRuleRepository commissionRuleRepo,
        IProfessionalRepository professionalRepo,
        IServiceRepository serviceRepo,
        ITenantService tenant)
    {
        _commissionRuleRepo = commissionRuleRepo;
        _professionalRepo = professionalRepo;
        _serviceRepo = serviceRepo;
        _tenant = tenant;
    }

    public async Task<Result<CommissionRuleDto>> Handle(CreateCommissionRuleCommand request, CancellationToken ct)
    {
        if (request.ProfessionalId.HasValue && request.ServiceId.HasValue)
        {
            var professional = await _professionalRepo.GetByIdAsync(request.ProfessionalId.Value, ct);
            if (professional is null)
                return Result<CommissionRuleDto>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");

            var service = await _serviceRepo.GetByIdAsync(request.ServiceId.Value, ct);
            if (service is null)
                return Result<CommissionRuleDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");

            var effective = await _commissionRuleRepo.GetEffectiveRuleAsync(
                _tenant.TenantId, request.ProfessionalId.Value, request.ServiceId.Value, ct);
            if (effective is not null)
                return Result<CommissionRuleDto>.Failure(
                    "COMMISSION_RULE_DUPLICATE", "A commission rule already exists for this professional and service combination.");
        }
        else if (request.ProfessionalId.HasValue)
        {
            var professional = await _professionalRepo.GetByIdAsync(request.ProfessionalId.Value, ct);
            if (professional is null)
                return Result<CommissionRuleDto>.Failure("PROFESSIONAL_NOT_FOUND", "Professional not found.");
        }
        else if (request.ServiceId.HasValue)
        {
            var service = await _serviceRepo.GetByIdAsync(request.ServiceId.Value, ct);
            if (service is null)
                return Result<CommissionRuleDto>.Failure("SERVICE_NOT_FOUND", "Service not found.");
        }

        var rule = CommissionRule.Create(
            _tenant.TenantId,
            request.CompanyPercentage,
            request.ProfessionalId,
            request.ServiceId);

        await _commissionRuleRepo.AddAsync(rule, ct);
        await _commissionRuleRepo.SaveChangesAsync(ct);

        return Result<CommissionRuleDto>.Success(rule.Adapt<CommissionRuleDto>());
    }
}
