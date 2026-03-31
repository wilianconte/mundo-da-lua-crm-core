namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record UpdateRoleInput(string Name, string? Description, bool? IsActive);
