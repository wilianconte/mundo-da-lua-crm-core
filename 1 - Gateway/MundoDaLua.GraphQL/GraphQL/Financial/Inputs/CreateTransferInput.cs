namespace MyCRM.GraphQL.GraphQL.Financial.Inputs;

public record CreateTransferInput(
    Guid     FromWalletId,
    Guid     ToWalletId,
    decimal  Amount,
    string   Description,
    Guid     CategoryId,
    Guid     PaymentMethodId,
    DateTime TransactionDate);
