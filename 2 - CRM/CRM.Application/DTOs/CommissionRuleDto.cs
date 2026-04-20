namespace MyCRM.CRM.Application.DTOs;

public record CommissionRuleDto(
    Guid Id,
    Guid TenantId,
    Guid? ProfessionalId,
    Guid? ServiceId,
    decimal CompanyPercentage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
