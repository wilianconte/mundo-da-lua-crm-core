namespace MyCRM.GraphQL.GraphQL.StudentCourses.Inputs;

public record CreateStudentCourseInput(
    Guid StudentId,
    Guid CourseId,
    DateOnly? EnrollmentDate,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ClassGroup,
    string? Shift,
    string? ScheduleDescription,
    Guid? UnitId,
    string? Notes
);
