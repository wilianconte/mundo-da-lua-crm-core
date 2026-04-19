namespace MyCRM.CRM.Application.DTOs;

public record CategoryDto(
    Guid            Id,
    Guid            TenantId,
    string          Name,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? UpdatedAt);
