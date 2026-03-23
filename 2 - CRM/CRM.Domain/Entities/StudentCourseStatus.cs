namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Lifecycle status of a student's enrollment in a course (StudentCourse).
/// </summary>
public enum StudentCourseStatus
{
    /// <summary>Enrollment is confirmed and the student is attending.</summary>
    Active = 1,

    /// <summary>Enrollment is created but awaiting confirmation or payment.</summary>
    Pending = 2,

    /// <summary>Student has successfully completed the course.</summary>
    Completed = 3,

    /// <summary>Enrollment was cancelled by the student or the institution.</summary>
    Cancelled = 4,

    /// <summary>Enrollment is temporarily suspended (e.g. financial issue, medical leave).</summary>
    Suspended = 5,
}
