namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record CreateRoleInput(
    string Name,
    string? Description,
    string[]? Permissions
);

public record UpdateRoleInput(
    string Name,
    string? Description,
    string[]? Permissions
);
