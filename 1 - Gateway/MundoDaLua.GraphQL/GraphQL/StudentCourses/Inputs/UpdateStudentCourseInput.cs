namespace MyCRM.GraphQL.GraphQL.StudentCourses.Inputs;

public record UpdateStudentCourseInput(
    DateOnly? EnrollmentDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ClassGroup,
    string? Shift,
    string? ScheduleDescription,
    Guid? UnitId,
    string? Notes
);
