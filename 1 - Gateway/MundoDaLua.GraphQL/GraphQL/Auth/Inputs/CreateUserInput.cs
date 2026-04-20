namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record CreateUserInput(
    string Name,
    string Email,
    string Password,
    Guid? PersonId,
    bool IsAdmin = false,
    IReadOnlyList<Guid>? RoleIds = null
);
