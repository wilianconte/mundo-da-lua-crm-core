namespace MyCRM.GraphQL.GraphQL.Students.Inputs;

public record CreateStudentInput(
    Guid PersonId,
    Guid? UnitId,
    string? Notes
);
