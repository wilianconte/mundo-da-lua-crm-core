using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantService _tenantService;
    private readonly ICrmTenantProvisioningService _crmProvisioning;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterTenantHandler> _logger;

    public RegisterTenantHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPasswordHasher passwordHasher,
        ITenantService tenantService,
        ICrmTenantProvisioningService crmProvisioning,
        IEmailSender emailSender,
        ILogger<RegisterTenantHandler> logger)
    {
        _tenantRepository     = tenantRepository;
        _userRepository       = userRepository;
        _roleRepository       = roleRepository;
        _permissionRepository = permissionRepository;
        _passwordHasher       = passwordHasher;
        _tenantService        = tenantService;
        _crmProvisioning      = crmProvisioning;
        _emailSender          = emailSender;
        _logger               = logger;
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

        // 3. Cria o Role "Administrador" para o novo tenant e atribui todas as permissões
        var adminRole = Role.Create(
            tenantId:    newTenantId,
            name:        "Administrador",
            description: "Acesso total ao sistema.");

        var allPermissions = await _permissionRepository.GetAllAsync(ct);
        adminRole.SyncPermissions(allPermissions.Select(p => p.Id).ToList());

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

        // 5. Envia email de boas-vindas (falha não impede o registro)
        try
        {
            var htmlBody = $"""
                <h1>Bem-vindo ao Mundo da Lua CRM, {request.AdminName}!</h1>
                <p>Sua empresa <strong>{request.CompanyLegalName}</strong> foi registrada com sucesso.</p>
                <p>Você já pode acessar o sistema com o e-mail <strong>{request.AdminEmail}</strong>.</p>
                <p>Qualquer dúvida, entre em contato com o suporte.</p>
                """;

            await _emailSender.SendAsync(new EmailMessage(
                To:       request.AdminEmail,
                Subject:  $"Bem-vindo ao Mundo da Lua CRM — {request.CompanyLegalName}",
                HtmlBody: htmlBody,
                ToName:   request.AdminName), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar email de boas-vindas para {Email}", request.AdminEmail);
        }

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
