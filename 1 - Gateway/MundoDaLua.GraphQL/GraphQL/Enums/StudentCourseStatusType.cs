using HotChocolate.Types;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Enums;

/// <summary>
/// GraphQL enum type for StudentCourseStatus with SCREAMING_SNAKE_CASE naming.
/// ACTIVE, PENDING, COMPLETED, CANCELLED, SUSPENDED
/// </summary>
public sealed class StudentCourseStatusType : EnumType<StudentCourseStatus>
{
    protected override void Configure(IEnumTypeDescriptor<StudentCourseStatus> descriptor)
    {
        descriptor.Name("StudentCourseStatus");

        descriptor.Value(StudentCourseStatus.Active)
            .Name("ACTIVE");

        descriptor.Value(StudentCourseStatus.Pending)
            .Name("PENDING");

        descriptor.Value(StudentCourseStatus.Completed)
            .Name("COMPLETED");

        descriptor.Value(StudentCourseStatus.Cancelled)
            .Name("CANCELLED");

        descriptor.Value(StudentCourseStatus.Suspended)
            .Name("SUSPENDED");
    }
}
