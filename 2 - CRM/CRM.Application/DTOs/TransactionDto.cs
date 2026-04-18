using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record TransactionDto(
    Guid            Id,
    Guid            TenantId,
    Guid            WalletId,
    Guid            CategoryId,
    Guid            PaymentMethodId,
    TransactionType Type,
    decimal         Amount,
    string          Description,
    DateTime        TransactionDate,
    bool            IsReconciled,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? UpdatedAt);
