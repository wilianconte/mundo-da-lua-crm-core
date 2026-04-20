namespace MyCRM.CRM.Application.DTOs;

public record PaymentMethodDto(
    Guid Id,
    Guid TenantId,
    string Name,
    Guid WalletId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
