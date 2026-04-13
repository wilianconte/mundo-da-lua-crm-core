namespace MyCRM.Shared.Kernel.Services;

/// <summary>
/// Resultado do provisionamento CRM durante o registro de um novo tenant.
/// </summary>
public record TenantCrmData(Guid CompanyId, Guid PersonId);
