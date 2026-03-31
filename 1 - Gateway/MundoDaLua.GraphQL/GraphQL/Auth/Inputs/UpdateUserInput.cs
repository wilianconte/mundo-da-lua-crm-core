namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record UpdateUserInput(
    string Name,
    string Email,
    Guid? PersonId,
    bool IsActive,
    string? Password,
    IReadOnlyList<Guid>? RoleIds = null
);
