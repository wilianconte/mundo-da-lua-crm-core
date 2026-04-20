using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record ProfessionalDto(
    Guid Id,
    Guid TenantId,
    Guid PersonId,
    ProfessionalStatus Status,
    string? Bio,
    string? LicenseNumber,
    decimal? CommissionPercentage,
    Guid? WalletId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
