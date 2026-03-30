namespace MyCRM.GraphQL.GraphQL.Students.Inputs;

public record UpdateStudentInput(
    string? RegistrationNumber,
    string? SchoolName,
    string? GradeOrClass,
    string? EnrollmentType,
    Guid? UnitId,
    string? ClassGroup,
    DateOnly? StartDate,
    string? Notes
);
