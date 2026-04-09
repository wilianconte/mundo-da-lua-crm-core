using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.Auth.Application.Commands.Tenants.RegisterTenant;

public sealed class RegisterTenantHandler : IRequestHandler<RegisterTenantCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantService _tenantService;
    private readonly ICrmTenantProvisioningService _crmProvisioning;

    public RegisterTenantHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITenantService tenantService,
        ICrmTenantProvisioningService crmProvisioning)
    {
        _tenantRepository = tenantRepository;
        _userRepository   = userRepository;
        _roleRepository   = roleRepository;
        _passwordHasher   = passwordHasher;
        _tenantService    = tenantService;
        _crmProvisioning  = crmProvisioning;
    }

    public async Task<Result<TenantDto>> Handle(RegisterTenantCommand request, CancellationToken ct)
    {
        // Gera o TenantId e define o contexto de tenant para toda a requisição.
        // ITenantService é Scoped — SetTenant aqui afeta AuthDbContext e CRMDbContext
        // na mesma requisição HTTP (sem JWT, sem claim tenant_id).
        var newTenantId = Guid.NewGuid();
        _tenantService.SetTenant(newTenantId);

        // 1. Provisiona Company + Person no CRM (schema separado, mesmo banco)
        TenantCrmData crmData;
        try
        {
            crmData = await _crmProvisioning.ProvisionAsync(
                tenantId:             newTenantId,
                companyLegalName:     request.CompanyLegalName,
                companyCnpj:          request.CompanyCnpj,
                companyEmail:         request.CompanyEmail,
                companyPhone:         request.CompanyPhone,
                personFullName:       request.AdminName,
                personEmail:          request.AdminEmail,
                personDocumentNumber: request.AdminCpf,
                personPhone:          request.AdminPhone,
                ct:                   ct);
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure("TENANT_CRM_PROVISION_FAILED",
                $"Failed to create company/person records: {ex.Message}");
        }

        // 2. Cria o Tenant com o Id pré-determinado (que será o TenantId do sistema)
        var tenant = Tenant.Create(
            name:          request.CompanyLegalName,
            companyId:     crmData.CompanyId,
            ownerPersonId: crmData.PersonId,
            id:            newTenantId);

        await _tenantRepository.AddAsync(tenant, ct);
        await _tenantRepository.SaveChangesAsync(ct);

        // 3. Cria o Role "Administrador" para o novo tenant
        var adminRole = Role.Create(
            tenantId:    newTenantId,
            name:        "Administrador",
            description: "Acesso total ao sistema.");

        await _roleRepository.AddAsync(adminRole, ct);
        await _roleRepository.SaveChangesAsync(ct);

        // 4. Cria o usuário de acesso vinculado à Person CRM e ao Role admin
        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(
            tenantId:     newTenantId,
            name:         request.AdminName,
            email:        request.AdminEmail,
            passwordHash: passwordHash,
            personId:     crmData.PersonId);

        user.SyncRoles([adminRole.Id]);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        return Result<TenantDto>.Success(new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.CompanyId,
            tenant.OwnerPersonId,
            tenant.Status,
            tenant.Plan,
            tenant.CreatedAt,
            tenant.UpdatedAt));
    }
}
