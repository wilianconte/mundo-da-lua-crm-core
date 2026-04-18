namespace MyCRM.CRM.Application.DTOs;

public record WalletDto(
    Guid            Id,
    Guid            TenantId,
    string          Name,
    decimal         Balance,
    decimal         InitialBalance,
    bool            IsActive,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? UpdatedAt);
