namespace MyCRM.CRM.Application.DTOs;

public record ProfessionalServiceDto(
    Guid Id,
    Guid TenantId,
    Guid ProfessionalId,
    Guid ServiceId,
    decimal? CustomPrice,
    int? CustomDurationInMinutes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
