namespace MyCRM.CRM.Application.DTOs;

public record ReconciliationDto(
    Guid            Id,
    Guid            TenantId,
    Guid            WalletId,
    Guid            TransactionId,
    string          ExternalId,
    decimal         ExternalAmount,
    DateTime        ExternalDate,
    DateTime        MatchedAt,
    DateTimeOffset  CreatedAt);
