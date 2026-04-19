namespace MyCRM.GraphQL.GraphQL.Financial.Inputs;

public record UpdateTransactionInput(
    decimal  Amount,
    string   Description,
    Guid     CategoryId,
    Guid     PaymentMethodId,
    DateTime TransactionDate);
