namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record LoginInput(Guid TenantId, string Email, string Password);
