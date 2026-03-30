namespace MyCRM.GraphQL.GraphQL.Students.Inputs;

public record UpdateStudentInput(
    Guid? UnitId,
    string? Notes
);
