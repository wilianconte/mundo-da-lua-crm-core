namespace MyCRM.GraphQL.GraphQL.Auth.Inputs;

public record AssignRoleToUserInput(
    Guid UserId,
    Guid RoleId
);

public record RemoveRoleFromUserInput(
    Guid UserId,
    Guid RoleId
);
