namespace MyCRM.CRM.Application.DTOs;

/// <summary>
/// Enrollment status derived from the student's active course enrollments.
/// ACTIVE  → exists at least one StudentCourse with Status = Active.
/// INACTIVE → no Active enrollment found (Pending does NOT count as active).
/// </summary>
public enum StudentEnrollmentStatus
{
    Active   = 1,
    Inactive = 2,
}
