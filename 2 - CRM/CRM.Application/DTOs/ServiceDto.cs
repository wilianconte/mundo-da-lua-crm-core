namespace MyCRM.CRM.Application.DTOs;

public record ServiceDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    decimal DefaultPrice,
    int DefaultDurationInMinutes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
