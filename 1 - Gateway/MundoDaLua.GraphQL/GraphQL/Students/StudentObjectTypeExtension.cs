using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Students;

/// <summary>
/// Extends the Student GraphQL type with the computed enrollmentStatus field.
/// ACTIVE  → has at least one StudentCourse with Status = ACTIVE
/// INACTIVE → no Active enrollment (PENDING does not count)
/// </summary>
[ExtendObjectType(typeof(Student))]
public sealed class StudentObjectTypeExtension
{
    /// <summary>
    /// Derived enrollment status. True source of truth is the StudentCourse table.
    /// </summary>
    [GraphQLName("enrollmentStatus")]
    public StudentEnrollmentStatus GetEnrollmentStatus([Parent] Student student) =>
        student.Courses.Any(c => c.Status == StudentCourseStatus.Active)
            ? StudentEnrollmentStatus.Active
            : StudentEnrollmentStatus.Inactive;
}
