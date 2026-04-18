using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Financial.Inputs;

public record CreateTransactionInput(
    Guid            WalletId,
    TransactionType Type,
    decimal         Amount,
    string          Description,
    Guid            CategoryId,
    Guid            PaymentMethodId,
    DateTime        TransactionDate);
