using HotChocolate.Data.Sorting;
using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Students.Types;

/// <summary>
/// Custom sort type for Student.
/// Note: Sorting by computed enrollmentStatus is not supported at the database level
/// because it depends on the Courses collection which requires complex SQL.
/// </summary>
public sealed class StudentSortType : SortInputType<Student>
{
    protected override void Configure(ISortInputTypeDescriptor<Student> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        // Standard fields (all sortable at database level)
        descriptor.Field(f => f.Id);
        descriptor.Field(f => f.PersonId);
        descriptor.Field(f => f.UnitId);
        descriptor.Field(f => f.CreatedAt);
        descriptor.Field(f => f.UpdatedAt);

        // Note: enrollmentStatus is intentionally excluded from sorting
        // because it's computed from the Courses collection and cannot be
        // translated to SQL for ordering. Use filtering by enrollmentStatus
        // instead, or sort in-memory after fetching results.
    }
}
