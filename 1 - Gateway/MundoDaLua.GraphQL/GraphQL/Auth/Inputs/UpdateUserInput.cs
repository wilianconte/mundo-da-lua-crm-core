namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record UpdateUserInput(
    string Name,
    string Email,
    Guid? PersonId,
    bool IsActive,
    bool IsAdmin,
    string? Password,
    IReadOnlyList<Guid>? RoleIds = null
);
