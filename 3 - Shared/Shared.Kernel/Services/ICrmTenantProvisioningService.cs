namespace MyCRM.Shared.Kernel.Services;

/// <summary>
/// Cria os registros CRM (Company + Person) para um novo tenant.
///
/// A implementação reside em CRM.Infrastructure. A interface fica em Shared.Kernel
/// para que Auth.Application possa depender dela sem criar referência circular.
/// </summary>
public interface ICrmTenantProvisioningService
{
    /// <summary>
    /// Cria uma Company e uma Person no schema CRM usando o <paramref name="tenantId"/> fornecido.
    /// Chama SetTenant internamente para que o CRMDbContext injete o TenantId correto.
    /// </summary>
    Task<TenantCrmData> ProvisionAsync(
        Guid tenantId,
        string companyLegalName,
        string? companyCnpj,
        string? companyEmail,
        string? companyPhone,
        string personFullName,
        string personEmail,
        string? personDocumentNumber,
        string? personPhone,
        CancellationToken ct = default);
}
