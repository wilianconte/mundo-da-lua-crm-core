namespace MyCRM.GraphQL.GraphQL.Financial.Inputs;

public record CreateWalletInput(string Name, decimal InitialBalance = 0);
