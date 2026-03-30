namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record RefreshTokenInput(Guid TenantId, string RefreshToken);
