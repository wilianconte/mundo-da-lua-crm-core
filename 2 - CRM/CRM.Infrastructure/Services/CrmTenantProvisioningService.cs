using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.CRM.Infrastructure.Services;

/// <summary>
/// Cria os registros CRM (Company + Person) durante o provisionamento de um novo tenant.
///
/// Chama tenantService.SetTenant() com o tenantId fornecido antes de salvar, garantindo
/// que o CRMDbContext.SaveChangesAsync() injete o TenantId correto nas entidades.
/// </summary>
public sealed class CrmTenantProvisioningService : ICrmTenantProvisioningService
{
    private readonly CRMDbContext _db;
    private readonly ITenantService _tenantService;

    public CrmTenantProvisioningService(CRMDbContext db, ITenantService tenantService)
    {
        _db            = db;
        _tenantService = tenantService;
    }

    public async Task<TenantCrmData> ProvisionAsync(
        Guid tenantId,
        string companyLegalName,
        string? companyCnpj,
        string? companyEmail,
        string? companyPhone,
        string personFullName,
        string personEmail,
        string? personDocumentNumber,
        string? personPhone,
        CancellationToken ct = default)
    {
        // Define o tenant antes de qualquer operação — necessário para que
        // CRMDbContext.SaveChangesAsync() injete o TenantId correto.
        _tenantService.SetTenant(tenantId);

        // Cria a Company
        var company = Company.Create(
            tenantId:          tenantId,
            legalName:         companyLegalName,
            registrationNumber: companyCnpj,
            email:             companyEmail,
            primaryPhone:      companyPhone);

        await _db.Companies.AddAsync(company, ct);

        // Cria a Person (administrador)
        var person = Person.Create(
            tenantId:       tenantId,
            fullName:       personFullName,
            email:          personEmail,
            documentNumber: personDocumentNumber,
            primaryPhone:   personPhone);

        await _db.People.AddAsync(person, ct);

        await _db.SaveChangesAsync(ct);

        return new TenantCrmData(company.Id, person.Id);
    }
}
