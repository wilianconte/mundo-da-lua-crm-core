namespace MyCRM.CRM.Application.DTOs;

public record PaymentMethodDto(
    Guid            Id,
    Guid            TenantId,
    string          Name,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? UpdatedAt);
