using HotChocolate.Types;
using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Enums;

/// <summary>
/// GraphQL enum type for StudentEnrollmentStatus with SCREAMING_SNAKE_CASE naming.
/// ACTIVE, INACTIVE
/// </summary>
public sealed class StudentEnrollmentStatusType : EnumType<StudentEnrollmentStatus>
{
    protected override void Configure(IEnumTypeDescriptor<StudentEnrollmentStatus> descriptor)
    {
        descriptor.Name("StudentEnrollmentStatus");

        descriptor.Value(StudentEnrollmentStatus.Active)
            .Name("ACTIVE");

        descriptor.Value(StudentEnrollmentStatus.Inactive)
            .Name("INACTIVE");
    }
}
