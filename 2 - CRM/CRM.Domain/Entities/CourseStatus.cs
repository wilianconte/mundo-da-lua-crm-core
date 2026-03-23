namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Lifecycle status of a Course offering.
/// </summary>
public enum CourseStatus
{
    /// <summary>Course is being planned and is not yet open for enrollment.</summary>
    Draft = 1,

    /// <summary>Course is active and accepting enrollments.</summary>
    Active = 2,

    /// <summary>Course is temporarily inactive (paused or suspended).</summary>
    Inactive = 3,

    /// <summary>Course has finished and all sessions were completed.</summary>
    Completed = 4,

    /// <summary>Course was cancelled before completion.</summary>
    Cancelled = 5,
}
