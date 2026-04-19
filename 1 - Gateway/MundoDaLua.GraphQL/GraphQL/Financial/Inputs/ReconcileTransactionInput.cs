namespace MyCRM.GraphQL.GraphQL.Financial.Inputs;

public record ReconcileTransactionInput(
    Guid     TransactionId,
    string   ExternalId,
    decimal  ExternalAmount,
    DateTime ExternalDate);
